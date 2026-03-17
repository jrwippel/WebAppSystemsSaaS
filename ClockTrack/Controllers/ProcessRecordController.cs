using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using ClockTrack.Filters;
using ClockTrack.Helper;
using ClockTrack.Models;
using ClockTrack.Models.Enums;
using ClockTrack.Services;
using ClockTrack.Data;
using Microsoft.EntityFrameworkCore;
using static ClockTrack.Helper.Sessao;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NPOIHorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;
using NPOIVerticalAlignment = NPOI.SS.UserModel.VerticalAlignment;



namespace ClockTrack.Controllers
{
    [PaginaParaUsuarioLogado]
   
    public class ProcessRecordController : Controller
    {
        private readonly ProcessRecordService _processRecordService;

        private readonly ClientService _clientService;

        private readonly AttorneyService _attorneyService;

        private readonly ValorClienteService _valorClienteService;

        private readonly IWebHostEnvironment _env;

        private readonly ISessao _isessao;

        private readonly ParametroService _parametroService;

        private readonly DepartmentService _departmentService;

        private readonly ClockTrackContext _context;

        public ProcessRecordController(ProcessRecordService processRecordService, ClientService clientService, AttorneyService attorneyService, IWebHostEnvironment env, ISessao isessao, 
            ValorClienteService valorClienteService, ParametroService parametroService, DepartmentService departmentService, ClockTrackContext context)
        {
            _processRecordService = processRecordService;
            _clientService = clientService;
            _attorneyService = attorneyService;
            _valorClienteService = valorClienteService;
            _env = env;
            _isessao = isessao;
            _parametroService = parametroService;
            _departmentService = departmentService;
            _context = context;
            
            // Configurar licença do QuestPDF (Community License)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                await PopulateViewBag();
                return View();
            }
            catch (SessionExpiredException)
            {
                // Redirecione para a página de login se a sessão expirou
                TempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";
                return RedirectToAction("Index", "Login");
            }
        }
        public async Task<IActionResult> SimpleSearch(DateTime? minDate, DateTime? maxDate, string clientIds, int? attorneyId, int? departmentId, int? activityTypeId)
        {
            SetDefaultDateValues(ref minDate, ref maxDate);

            // Converter string de IDs separados por vírgula em lista de inteiros
            List<int> clientIdList = null;
            if (!string.IsNullOrEmpty(clientIds))
            {
                clientIdList = clientIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();
            }

            PopulateViewData(minDate, maxDate, clientIdList, attorneyId, activityTypeId?.ToString());
            await PopulateViewBag();

            var result = await _processRecordService.FindByDateAsync(minDate, maxDate, clientIdList, attorneyId, departmentId, activityTypeId);
            return View(result);

        }

        // Ação para gerar e baixar o arquivo CSV

        public async Task<IActionResult> DownloadReport(DateTime? minDate, DateTime? maxDate, string clientIds, int? attorneyId, int? departmentId, int? activityTypeId = null, string format = "xlsx")
        {

            int? activityTypeIdValue = activityTypeId;

            // Converter string de IDs separados por vírgula em lista de inteiros
            List<int> clientIdList = null;
            if (!string.IsNullOrEmpty(clientIds))
            {
                clientIdList = clientIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();
            }

            // Obter os registros filtrados usando a função FindByDateAsync
            var filteredRecords = await _processRecordService.FindByDateAsync(minDate, maxDate, clientIdList, attorneyId, departmentId, activityTypeIdValue);

            string clientName = null;
            if (clientIdList != null && clientIdList.Count == 1)
            {
                var client = await _clientService.FindByIdAsync(clientIdList.First());
                if (client != null)
                {
                    clientName = client.Name;
                }
            }
            else if (clientIdList != null && clientIdList.Count > 1)
            {
                clientName = "Multiplos_Clientes";
            }

            if (format == "csv")
            {
                // Construir o conteúdo do arquivo CSV
                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("Data;Usuario;Cliente;Atividade;Hora Inicio;Hora Final;Horas Trabalhadas;Area");
                foreach (var item in filteredRecords)
                {
                    csvContent.AppendLine($"{item.Date.ToString("dd/MM/yyyy")};{item.Attorney.Name};{item.Client.Name};{item.Description};{(int)item.HoraInicial.TotalHours}:{item.HoraInicial.Minutes:00};{(int)item.HoraFinal.TotalHours}:{item.HoraFinal.Minutes:00};{item.CalculoHoras()};{item.Department.Name}");
                }
                // Converter o conteúdo do CSV em bytes
                byte[] bytes = Encoding.GetEncoding("Windows-1252").GetBytes(csvContent.ToString());

                // Definir o nome do arquivo CSV para download
                string fileName = "exported_data.csv";

                // Retornar o arquivo CSV como resposta para download
                return File(bytes, "text/csv", fileName);
            }
            else if (format == "xlsx")
            {
                var workbook = new XSSFWorkbook();

                string startDateString = minDate?.ToString("ddMMyyyy") ?? "NoStart";
                string endDateString = maxDate?.ToString("ddMMyyyy") ?? "NoEnd";
                string sheetName = $"{startDateString}_{endDateString}";



                // Verifique se o nome da planilha é menor que 31 caracteres
                if (sheetName.Length > 31)
                {
                    sheetName = sheetName.Substring(0, 31);
                }
                var sheet = workbook.CreateSheet(sheetName);

                // Criar o estilo de célula com quebra de texto
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.WrapText = true;

                // Criar o estilo de sombreamento com XSSF
                XSSFCellStyle shadedStyle = (XSSFCellStyle)workbook.CreateCellStyle();

                // Definindo a cor azul claro Ênfase 1 mais claro 80% em RGB
                XSSFColor lightBlueEmphasis = new XSSFColor(new byte[] { 222, 235, 247 });

                // Aplicar a cor ao estilo da célula
                shadedStyle.SetFillForegroundColor(lightBlueEmphasis);
                shadedStyle.FillPattern = FillPattern.SolidForeground;

                // Aplicar o estilo de sombreamento às células
                for (int i = 0; i <= 4; i++)
                {
                    IRow row = sheet.GetRow(i) ?? sheet.CreateRow(i);
                    for (int j = 0; j <= 9; j++)
                    {
                        ICell cell = row.GetCell(j) ?? row.CreateCell(j);
                        cell.CellStyle = shadedStyle; // Aplique o estilo com o azul claro ênfase 1
                    }
                }


                // Criar o estilo de cabeçalho
                ICellStyle headerStyle = workbook.CreateCellStyle();
                headerStyle.FillForegroundColor = IndexedColors.Black.Index;  // Definir a cor de fundo para preto
                headerStyle.FillPattern = FillPattern.SolidForeground;  // Padrão de preenchimento sólido

                // Criar a fonte para o cabeçalho
                IFont font = workbook.CreateFont();
                font.Color = IndexedColors.White.Index;  // Definir a cor da fonte para branco
                font.Boldweight = (short)FontBoldWeight.Bold;  // Deixar o texto em negrito
                headerStyle.SetFont(font);

                // Centralizar o texto no cabeçalho
                headerStyle.Alignment = NPOIHorizontalAlignment.Center;
                headerStyle.VerticalAlignment = NPOIVerticalAlignment.Center;

                // Criar o cabeçalho na linha 8
                var headerRow = sheet.CreateRow(5);

                // Criar as células do cabeçalho e aplicar o estilo
                string[] headers = { "Data", "Responsável", "Solicitante", "Cliente", "Tipo", "Descrição", "Hora Inicial", "Hora Final", "Horas Trabalhadas", "Área" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                // Adicionar dados ao arquivo Excel
                int rowNum = 6;  // Começando da linha 9, porque a primeira até a oitava são da imagem e o cabeçalho
                var rowTotal = sheet.CreateRow(rowNum);  // Crie a linha do total
                double totalHoras = 0;

                Dictionary<string, (double hours, double value)> departmentSummary = new Dictionary<string, (double hours, double value)>();


                // Cria um estilo para as células com texto justificado e centralizado (para as outras colunas)
                ICellStyle justifiedCellStyle = workbook.CreateCellStyle();
                justifiedCellStyle.WrapText = true; // Permite a quebra de linha dentro da célula
                justifiedCellStyle.Alignment = NPOIHorizontalAlignment.Center; // Alinhamento central horizontal
                justifiedCellStyle.VerticalAlignment = NPOIVerticalAlignment.Center; // Alinhamento central vertical

                // Definindo a cor azul claro Ênfase 1 mais claro 80% em RGB
                //XSSFColor lightBlueEmphasis = new XSSFColor(new byte[] { 222, 235, 247 });

                // Cria um estilo que combina justificação, sombreamento e centralização (para as outras colunas)
                ICellStyle justifiedShadedStyle = workbook.CreateCellStyle();
                justifiedShadedStyle.CloneStyleFrom(justifiedCellStyle); // Copia as configurações de justificação e centralização
                ((XSSFCellStyle)justifiedShadedStyle).SetFillForegroundColor(lightBlueEmphasis); // Define a cor de fundo como azul claro Ênfase 1
                justifiedShadedStyle.FillPattern = FillPattern.SolidForeground; // Padrão de preenchimento sólido

                // Cria um estilo específico para a coluna 5 (alinhamento à esquerda e no topo)
                ICellStyle justifiedLeftTopStyle = workbook.CreateCellStyle();
                justifiedLeftTopStyle.WrapText = true; // Permite quebra de linha dentro da célula
                justifiedLeftTopStyle.Alignment = NPOIHorizontalAlignment.Left; // Alinhamento à esquerda
                justifiedLeftTopStyle.VerticalAlignment = NPOIVerticalAlignment.Top; // Alinhamento vertical no topo

                // Cria um estilo para a coluna 5 com sombreamento azul claro Ênfase 1
                ICellStyle justifiedLeftTopShadedStyle = workbook.CreateCellStyle();
                justifiedLeftTopShadedStyle.CloneStyleFrom(justifiedLeftTopStyle); // Copia o estilo de alinhamento à esquerda e no topo
                ((XSSFCellStyle)justifiedLeftTopShadedStyle).SetFillForegroundColor(lightBlueEmphasis); // Define a cor de fundo como azul claro Ênfase 1
                justifiedLeftTopShadedStyle.FillPattern = FillPattern.SolidForeground; // Padrão de preenchimento sólido



                // Definindo o tamanho da coluna 5 com uma largura fixa
                int columnIndex = 5; // coluna 5 (índice começa em 0)
                int columannWidth = 10000; // largura da coluna (o valor é em unidades de 1/256 da largura de um caractere)
                sheet.SetColumnWidth(columnIndex, columannWidth);


                for (int i = 0; i < filteredRecords.Count(); i++)
                {
                    var item = filteredRecords[i];
                    var row = sheet.CreateRow(rowNum);

                    // Crie células para todas as colunas
                    for (int column = 0; column < 10; column++)
                    {
                        row.CreateCell(column);
                    }

                    row.GetCell(0).SetCellValue(item.Date.ToString("dd/MM/yyyy"));
                    row.GetCell(1).SetCellValue(item.Attorney.Name);
                    row.GetCell(2).SetCellValue(item.Solicitante);
                    row.GetCell(3).SetCellValue(item.Client.Name);
                    row.GetCell(4).SetCellValue(item.ActivityType?.Name ?? "N/A");
                    // row.GetCell(5).SetCellValue(item.Description);

                    // Definindo o valor da célula na coluna 5 e aplicando o estilo justificado à esquerda e no topo
                    ICell descriptionCell = row.GetCell(columnIndex);
                    descriptionCell.SetCellValue(item.Description);

                    row.GetCell(6).SetCellValue(item.HoraInicial.ToString(@"hh\:mm"));
                    row.GetCell(7).SetCellValue(item.HoraFinal.ToString(@"hh\:mm"));

                    // Verifica se o tipo de registro é "Deslocamento" para considerar apenas 50% das horas
                    double horasCalculadas = item.CalculoHorasDecimal();
                    if (item.ActivityType?.Name?.Equals("Deslocamento", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        horasCalculadas *= 0.5;
                    }

                    // Converter horas decimais para formato hh:mm
                    int horasItem = (int)horasCalculadas;
                    int minutosItem = (int)Math.Round((horasCalculadas - horasItem) * 60);
                    string horasFormatadas = string.Format("{0}:{1:00}", horasItem, minutosItem);
                    row.GetCell(8).SetCellValue(horasFormatadas);


                    //row.GetCell(7).SetCellValue(item.Department.Name);
                    string departmentName = item.Department != null ? item.Department.Name : "N/A";
                    row.GetCell(9).SetCellValue(departmentName);

                    //totalHoras += item.CalculoHorasDecimal();

                    double totalHorasCalculadas = item.CalculoHorasDecimal();

                    // Se for "Deslocamento", aplica 50% apenas para esse item
                    if (item.ActivityType?.Name?.Equals("Deslocamento", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        totalHorasCalculadas *= 0.5;
                    }

                    // Adiciona ao total corretamente
                    totalHoras += totalHorasCalculadas;





                    // Aplique o estilo correto: justificado com ou sem sombreamento, dependendo se a linha é ímpar ou par
                    for (int j = 0; j < 10; j++)
                    {
                        if (i % 2 != 0) // Linhas ímpares
                        {
                            if (j == 5) // Coluna 5 com alinhamento à esquerda e no topo com sombreamento
                            {
                                row.GetCell(j).CellStyle = justifiedLeftTopShadedStyle;
                            }
                            else // Outras colunas com sombreamento e centralizadas
                            {
                                row.GetCell(j).CellStyle = justifiedShadedStyle;
                            }
                        }
                        else // Linhas pares
                        {
                            if (j == 5) // Coluna 5 com alinhamento à esquerda e no topo sem sombreamento
                            {
                                row.GetCell(j).CellStyle = justifiedLeftTopStyle;
                            }
                            else // Outras colunas sem sombreamento e centralizadas
                            {
                                row.GetCell(j).CellStyle = justifiedCellStyle;
                            }
                        }
                    }

                    if (!departmentSummary.ContainsKey(departmentName))
                    {
                        departmentSummary[departmentName] = (0, 0);
                    }

                    double hours = item.CalculoHorasDecimal();

                    if (item.ActivityType?.Name?.Equals("Deslocamento", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        hours *= 0.5;
                    }

                    var valorCliente = await _valorClienteService.GetValorForClienteAndUserAsync(item.ClientId, item.Attorney.Id); // supondo que haja um método que retorna o valor baseado no Cliente e Usuario
                    double value = 0;
                    if (valorCliente != null)
                    {
                        double valuePerHour = valorCliente.Valor;
                        value = hours * valuePerHour;
                    }
                    departmentSummary[departmentName] = (departmentSummary[departmentName].hours + hours, departmentSummary[departmentName].value + value);

                    rowNum++;
                }

                // Define where the summary should start
                int summaryStartRow = rowNum + 2;

                // Create the header row for the summary
                IRow summaryHeaderRow = sheet.CreateRow(summaryStartRow);

                // Convertendo o total de horas em minutos e arredondando para o número mais próximo de minutos.
                totalHoras = Math.Round(totalHoras * 60) / 60;
                int totalMinutos = (int)Math.Round(totalHoras * 60);
                int horasInteiras = totalMinutos / 60;
                int minutosRestantes = totalMinutos % 60;

                string totalHorasFormatado = string.Format("{0}:{1:00}", horasInteiras, minutosRestantes);

                // Criação da linha com o total de horas
                var totalRow = sheet.CreateRow(rowNum);
                totalRow.CreateCell(0).SetCellValue("Total de horas");
                totalRow.GetCell(0).CellStyle = headerStyle;  // Sombreado em preto com fonte branca
                totalRow.CreateCell(7).SetCellValue(totalHorasFormatado);
                totalRow.GetCell(7).CellStyle = headerStyle;





                // Desativa as linhas de grade
                sheet.DisplayGridlines = false;

                //   for (int columnNum = 0; columnNum < 10; columnNum++)
                //   {
                //       sheet.AutoSizeColumn(columnNum);
                //   }

                for (int columnNum = 0; columnNum < 10; columnNum++)
                {
                    if (columnNum != 5) // Não aplicar AutoSize na coluna 5
                    {
                        sheet.AutoSizeColumn(columnNum);
                    }
                }


                // Crie células para todas as colunas na linha de total
                for (int column = 0; column < 10; column++)
                {
                    totalRow.CreateCell(column);
                }

                totalRow.GetCell(0).SetCellValue("Total de horas");
                totalRow.GetCell(0).CellStyle = headerStyle;  // Sombreado em preto com fonte branca


                totalRow.GetCell(8).SetCellValue(totalHorasFormatado);
                totalRow.GetCell(8).CellStyle = headerStyle;  // Sombreado em preto com fonte branca

                // Aplicar o estilo de cabeçalho à linha de total
                for (int j = 0; j < 10; j++)
                {
                    totalRow.GetCell(j).CellStyle = headerStyle;
                }

                // Create the header row for the summary
                summaryHeaderRow = sheet.CreateRow(summaryStartRow);
                summaryHeaderRow.CreateCell(0).SetCellValue("Área");
                summaryHeaderRow.GetCell(0).CellStyle = headerStyle;
                summaryHeaderRow.CreateCell(1).SetCellValue("Horas");
                summaryHeaderRow.GetCell(1).CellStyle = headerStyle;
                summaryHeaderRow.CreateCell(2).SetCellValue("Valor");
                summaryHeaderRow.GetCell(2).CellStyle = headerStyle;

                // Print the summary data
                int summaryDataRow = summaryStartRow + 1;

                double totalHoursSummary = 0;
                double totalValueSummary = 0;

                CultureInfo brazilianCulture = new CultureInfo("pt-BR");
                foreach (var kvp in departmentSummary)
                {
                    IRow row = sheet.CreateRow(summaryDataRow);
                    row.CreateCell(0).SetCellValue(kvp.Key);
                    double hours = kvp.Value.hours;
                    totalHoursSummary += hours;  // add to total hours summary

                    // Convertendo o total de horas em minutos e arredondando para o número mais próximo de minutos.
                    int totalMinutes = (int)Math.Round(hours * 60);
                    int wholeHours = totalMinutes / 60;
                    int remainingMinutes = totalMinutes % 60;

                    string formattedHours = string.Format("{0}:{1:00}", wholeHours, remainingMinutes);


                    row.CreateCell(1).SetCellValue(formattedHours);

                    double value = kvp.Value.value;
                    totalValueSummary += value;  // add to total value summary
                                                 //row.CreateCell(2).SetCellValue(value);

                    row.CreateCell(2).SetCellValue(value.ToString("N2", brazilianCulture));
                    summaryDataRow++;
                }

                // Print the total summary
                IRow totalSummaryRow = sheet.CreateRow(summaryDataRow);
                totalSummaryRow.CreateCell(0).SetCellValue("Total");
                totalSummaryRow.GetCell(0).CellStyle = headerStyle;  // Apply the header style to total row

                double totalHours = Math.Round(totalHoursSummary * 60) / 60;
                int totalMinutesSummary = (int)Math.Round(totalHours * 60);
                int wholeHoursSummary = totalMinutesSummary / 60;
                int remainingMinutesSummary = totalMinutesSummary % 60;
                string formattedTotalHours = string.Format("{0}:{1:00}", wholeHoursSummary, remainingMinutesSummary);
                totalSummaryRow.CreateCell(1).SetCellValue(formattedTotalHours);
                totalSummaryRow.GetCell(1).CellStyle = headerStyle;  // Apply the header style to total row

                //totalSummaryRow.CreateCell(2).SetCellValue(totalValueSummary);
                totalSummaryRow.CreateCell(2).SetCellValue(totalValueSummary.ToString("N2", brazilianCulture));

                totalSummaryRow.GetCell(2).CellStyle = headerStyle;  // Apply the header style to total row
                /*
                var imagePath = System.IO.Path.Combine(_env.WebRootPath, "images", "LogoRelatorio.png");
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

                int pictureIdx = workbook.AddPicture(imageBytes, PictureType.PNG);
                var helper = workbook.GetCreationHelper();
                var drawing = sheet.CreateDrawingPatriarch();
                var anchor = helper.CreateClientAnchor();
                // Defina a posição da imagem
                anchor.Col1 = 0;
                anchor.Row1 = 1;  // A imagem começa na segunda linha
                var picture = drawing.CreatePicture(anchor, pictureIdx);
                picture.Resize(4);  // A imagem vai ocupar 7 linhas

                */
                var (imageBytes, mimeType, width, height) = await _parametroService.GetLogoAsync();

                int pictureIdx = workbook.AddPicture(imageBytes, PictureType.PNG);
                var helper = workbook.GetCreationHelper();
                var drawing = sheet.CreateDrawingPatriarch();
                var anchor = helper.CreateClientAnchor();

                // Defina a posição da imagem e ajuste o tamanho conforme as configurações
                anchor.Col1 = 0; // Defina a coluna inicial
                anchor.Row1 = 1; // Defina a linha inicial
                anchor.Col2 = anchor.Col1 + width; // Defina a coluna final com base na largura do logo
                anchor.Row2 = anchor.Row1 + height; // Defina a linha final com base na altura do logo

                var picture = drawing.CreatePicture(anchor, pictureIdx);
                picture.Resize(4);



                // Criar o estilo para a palavra "TimeSheet" com fonte de tamanho 30 e negrito
                ICellStyle timeSheetStyle = workbook.CreateCellStyle();
                IFont font1 = workbook.CreateFont();
                font1.FontHeightInPoints = 30;  // Define o tamanho da fonte como 30
                font1.Boldweight = (short)FontBoldWeight.Bold; // Define a fonte como negrito
                timeSheetStyle.SetFont(font1);

                // Centralizar o texto na célula
                timeSheetStyle.Alignment = NPOIHorizontalAlignment.Center;
                timeSheetStyle.VerticalAlignment = NPOIVerticalAlignment.Center;

                // Adicionar a palavra "TimeSheet" na célula (coluna 6, linha 3)
                var row3 = sheet.GetRow(2) ?? sheet.CreateRow(2); // Linha 3 é índice 2 (começa do 0)
                var cell3 = row3.GetCell(5) ?? row3.CreateCell(5); // Coluna 6 é índice 5
                cell3.SetCellValue("TimeSheet");
                cell3.CellStyle = timeSheetStyle; // Aplicar o estilo à célula

                // Agora copiar o estilo de sombreamento, se necessário
                var previousRow = sheet.GetRow(1); // Pega a linha 2 para copiar o estilo de uma célula
                if (previousRow != null)
                {
                    var previousCell = previousRow.GetCell(5); // Pega a célula da mesma coluna
                    if (previousCell != null)
                    {
                        var previousStyle = previousCell.CellStyle;
                        if (previousStyle != null)
                        {
                            // Clonar o estilo de sombreamento sem afetar a fonte
                            ICellStyle clonedStyle = workbook.CreateCellStyle();
                            clonedStyle.CloneStyleFrom(previousStyle);
                            clonedStyle.SetFont(font1); // Manter a fonte definida
                            clonedStyle.Alignment = NPOIHorizontalAlignment.Center; // Manter centralizado
                            clonedStyle.VerticalAlignment = NPOIVerticalAlignment.Center; // Manter centralizado
                            cell3.CellStyle = clonedStyle; // Aplicar o estilo clonado à célula
                        }
                    }
                }


                // Adicionar a imagem do cliente ao relatório Excel
                byte[] clientImageData = null;
                string clientImageMimeType = null;
                if (clientIdList != null && clientIdList.Count == 1)
                {
                    var client = await _clientService.FindByIdAsync(clientIdList.First());
                    if (client != null)
                    {
                        clientImageData = client.ImageData;
                        clientImageMimeType = client.ImageMimeType;
                    }
                }

                
                if (clientImageData != null)
                {
                    var clientSheet = workbook.GetSheet(sheetName);
                    var clientDrawing = clientSheet.CreateDrawingPatriarch();  // Renomeie a variável para evitar conflito
                    var clientAnchor = helper.CreateClientAnchor();
                    clientAnchor.Col1 = 7;  // Inicia na coluna 8
                    clientAnchor.Row1 = 1;  // A imagem começa na segunda linha

                    // Adicionar a imagem do cliente à planilha
                    int clientPictureIdx = workbook.AddPicture(clientImageData, GetPictureType(clientImageMimeType));
                    var clientPicture = clientDrawing.CreatePicture(clientAnchor, clientPictureIdx);  
                    clientPicture.Resize(1);  // A imagem vai ocupar 3 colunas

                    // Ajuste da altura da imagem para ocupar até a linha 7
                    clientAnchor.Row2 = 6;  // Termina na linha 7
                    

                }

                string fileName = "Relatório_TimeSheet";
                if (!string.IsNullOrEmpty(clientName))
                {
                    fileName += $"_{clientName}";
                }
                fileName += ".xlsx";     




                // Para retornar como um arquivo para download
                using (var stream = new MemoryStream())
                {
                    workbook.Write(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
            else
            {
                // Se o formato não for "csv" nem "xlsx", retorne um erro
                return BadRequest("Formato inválido");
            }
        }

        // Ação para gerar e baixar o arquivo PDF
        public async Task<IActionResult> DownloadPdfReport(DateTime? minDate, DateTime? maxDate, string clientIds, int? attorneyId, int? departmentId, int? activityTypeId = null)
        {

            // Converter string de IDs separados por vírgula em lista de inteiros
            List<int> clientIdList = null;
            if (!string.IsNullOrEmpty(clientIds))
            {
                clientIdList = clientIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();
            }

            // Obter os registros filtrados
            var filteredRecords = await _processRecordService.FindByDateAsync(minDate, maxDate, clientIdList, attorneyId, departmentId, activityTypeId);

            // Calcular total de horas
            TimeSpan totalHours = TimeSpan.Zero;
            foreach (var item in filteredRecords)
            {
                totalHours += item.CalculoHorasTotal();
            }
            int totalDays = (int)totalHours.TotalDays;
            TimeSpan correctedTotalHours = totalHours - TimeSpan.FromDays(totalDays);
            string totalFormatted = $"{totalDays * 24 + correctedTotalHours.Hours}:{correctedTotalHours.Minutes:00}";

            // Nome do cliente para o arquivo
            string clientName = "Todos";
            if (clientIdList != null && clientIdList.Count == 1)
            {
                var client = await _clientService.FindByIdAsync(clientIdList.First());
                if (client != null)
                {
                    clientName = client.Name;
                }
            }
            else if (clientIdList != null && clientIdList.Count > 1)
            {
                clientName = "Multiplos_Clientes";
            }

            // Gerar PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Relatório de Horas")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Grey.Darken4);
                            
                            column.Item().Text($"Período: {minDate?.ToString("dd/MM/yyyy")} a {maxDate?.ToString("dd/MM/yyyy")}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                        });

                        row.ConstantItem(120).AlignRight().Column(column =>
                        {
                            column.Item().Background(Colors.Purple.Medium)
                                .Padding(10)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("Total: ").FontColor(Colors.White).FontSize(10);
                                    text.Span(totalFormatted).FontColor(Colors.White).FontSize(16).Bold();
                                });
                        });
                    });

                    // Content
                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        // Definir colunas
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);  // Data
                            columns.RelativeColumn(2);   // Usuário
                            columns.RelativeColumn(1.5f); // Área
                            columns.ConstantColumn(50);  // Horas
                            columns.RelativeColumn(2);   // Cliente
                            columns.RelativeColumn(2);   // Solicitante
                            columns.RelativeColumn(1.5f); // Tipo
                        });

                        // Header da tabela
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Data").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Usuário").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Área").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Horas").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Solicitante").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5).Text("Tipo").FontColor(Colors.White).Bold();
                        });

                        // Dados
                        int rowIndex = 0;
                        foreach (var item in filteredRecords)
                        {
                            var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            
                            table.Cell().Background(bgColor).Padding(5).Text(item.Date.ToString("dd/MM/yyyy"));
                            table.Cell().Background(bgColor).Padding(5).Text(item.Attorney.Name);
                            table.Cell().Background(bgColor).Padding(5).Text(item.Department.Name);
                            table.Cell().Background(bgColor).Padding(5).Text(item.CalculoHoras());
                            table.Cell().Background(bgColor).Padding(5).Text(item.Client.Name);
                            table.Cell().Background(bgColor).Padding(5).Text(item.Solicitante);
                            table.Cell().Background(bgColor).Padding(5).Text(item.ActivityType?.Name ?? "N/A");
                            
                            rowIndex++;
                        }
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            });

            // Gerar o PDF em memória
            var pdfBytes = document.GeneratePdf();
            
            string fileName = $"Relatorio_Horas_{clientName}_{DateTime.Now:yyyyMMdd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }

        private PictureType GetPictureType(string mimeType)
        {
            switch (mimeType)
            {
                case "image/png":
                    return PictureType.PNG;
                case "image/jpeg":
                    return PictureType.JPEG;
                // Add more cases for other image types if needed
                default:
                    return PictureType.PNG; // Default to PNG if the type is not recognized
            }
        }


        public async Task<IActionResult> GerarPreFatura(DateTime? minDate, DateTime? maxDate, string clientIds, int? attorneyId, int? departmentId, int? activityTypeId = null)
        {
            List<int> clientIdList = null;
            if (!string.IsNullOrEmpty(clientIds))
            {
                clientIdList = clientIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();
            }

            var records = await _processRecordService.FindByDateAsync(minDate, maxDate, clientIdList, attorneyId, departmentId, activityTypeId);

            if (!records.Any())
            {
                TempData["MensagemAviso"] = "Nenhum registro encontrado para os filtros selecionados.";
                return RedirectToAction("Index");
            }

            // Buscar logo do escritório
            var tenantName = HttpContext.Session.GetString("TenantName") ?? "Escritório";
            byte[] logoBytes = null;
            try { (logoBytes, _, _, _) = await _parametroService.GetLogoAsync(); } catch { }


            // Agrupar por cliente e calcular totais
            var gruposPorCliente = records
                .GroupBy(r => r.Client)
                .Select(g => new
                {
                    Cliente = g.Key,
                    Registros = g.ToList(),
                    TotalHoras = g.Sum(r => r.CalculoHorasDecimal()),
                    TotalValor = g.Sum(r =>
                    {
                        var vc = _valorClienteService.GetValorForClienteAndUserAsync(r.ClientId, r.AttorneyId).Result;
                        return vc != null ? r.CalculoHorasDecimal() * vc.Valor : 0;
                    })
                })
                .ToList();

            double totalGeralHoras = gruposPorCliente.Sum(g => g.TotalHoras);
            double totalGeralValor = gruposPorCliente.Sum(g => g.TotalValor);

            var culture = new System.Globalization.CultureInfo("pt-BR");

            string FormatHoras(double h)
            {
                int hh = (int)h;
                int mm = (int)Math.Round((h - hh) * 60);
                return $"{hh}:{mm:00}";
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                // Logo ou nome do escritório
                                if (logoBytes != null && logoBytes.Length > 0)
                                {
                                    c.Item().MaxHeight(50).MaxWidth(160).Image(logoBytes);
                                    c.Item().PaddingTop(4).Text("Pré-Fatura de Honorários").FontSize(11).FontColor("#667eea");
                                }
                                else
                                {
                                    c.Item().Text(tenantName).FontSize(18).Bold().FontColor("#2d3748");
                                    c.Item().Text("Pré-Fatura de Honorários").FontSize(11).FontColor("#667eea");
                                }
                                c.Item().PaddingTop(4).Text($"Período: {minDate?.ToString("dd/MM/yyyy") ?? "—"} a {maxDate?.ToString("dd/MM/yyyy") ?? "—"}")
                                    .FontSize(9).FontColor("#718096");
                                c.Item().Text($"Emissão: {DateTime.Now:dd/MM/yyyy}").FontSize(9).FontColor("#718096");
                            });

                            row.ConstantItem(110).AlignRight().Column(c =>
                            {
                                c.Item().Background("#667eea").Padding(10).AlignCenter().Column(inner =>
                                {
                                    inner.Item().Text("TOTAL GERAL").FontSize(8).Bold().FontColor(Colors.White);
                                    inner.Item().Text(totalGeralValor.ToString("C2", culture)).FontSize(14).Bold().FontColor(Colors.White);
                                    inner.Item().Text(FormatHoras(totalGeralHoras) + "h").FontSize(9).FontColor("#e2e8f0");
                                });
                            });
                        });

                        col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor("#667eea");
                    });

                    // Content
                    page.Content().PaddingTop(12).Column(mainCol =>
                    {
                        foreach (var grupo in gruposPorCliente)
                        {
                            // Cabeçalho do cliente
                            mainCol.Item().Background(Colors.Grey.Lighten5).Border(1).BorderColor("#e2e8f0")
                                .Padding(8).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(grupo.Cliente.Name).FontSize(11).Bold().FontColor("#2d3748");
                                        if (!string.IsNullOrEmpty(grupo.Cliente.Document))
                                            c.Item().Text($"Doc: {grupo.Cliente.Document}").FontSize(8).FontColor("#718096");
                                    });
                                    row.ConstantItem(160).AlignRight().Column(c =>
                                    {
                                        c.Item().Text($"Total horas: {FormatHoras(grupo.TotalHoras)}h").FontSize(9).FontColor("#4a5568");
                                        c.Item().Text($"Valor: {grupo.TotalValor.ToString("C2", culture)}").FontSize(10).Bold().FontColor("#667eea");
                                    });
                                });

                            // Tabela de registros do cliente
                            mainCol.Item().PaddingBottom(12).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(60);   // Data
                                    cols.RelativeColumn(1.5f); // Advogado
                                    cols.RelativeColumn(3);    // Descrição
                                    cols.ConstantColumn(40);   // Horas
                                    cols.ConstantColumn(70);   // Valor
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Background("#764ba2").Padding(4).Text("Data").FontColor(Colors.White).Bold().FontSize(8);
                                    h.Cell().Background("#764ba2").Padding(4).Text("Responsável").FontColor(Colors.White).Bold().FontSize(8);
                                    h.Cell().Background("#764ba2").Padding(4).Text("Descrição").FontColor(Colors.White).Bold().FontSize(8);
                                    h.Cell().Background("#764ba2").Padding(4).AlignCenter().Text("Horas").FontColor(Colors.White).Bold().FontSize(8);
                                    h.Cell().Background("#764ba2").Padding(4).AlignRight().Text("Valor").FontColor(Colors.White).Bold().FontSize(8);
                                });

                                int idx = 0;
                                foreach (var r in grupo.Registros)
                                {
                                    var bg = idx % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                                    var vc = _valorClienteService.GetValorForClienteAndUserAsync(r.ClientId, r.AttorneyId).Result;
                                    double horas = r.CalculoHorasDecimal();
                                    double valor = vc != null ? horas * vc.Valor : 0;

                                    table.Cell().Background(bg).Padding(4).Text(r.Date.ToString("dd/MM/yy")).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).Text(r.Attorney.Name).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).Text(r.Description).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).AlignCenter().Text(FormatHoras(horas)).FontSize(8);
                                    table.Cell().Background(bg).Padding(4).AlignRight().Text(valor > 0 ? valor.ToString("C2", culture) : "—").FontSize(8);
                                    idx++;
                                }
                            });
                        }

                        // Rodapé de totais
                        mainCol.Item().LineHorizontal(1).LineColor("#667eea");
                        mainCol.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Text("* Documento sem valor fiscal. Sujeito a revisão.").FontSize(8).FontColor("#a0aec0").Italic();
                            row.ConstantItem(200).AlignRight().Text(text =>
                            {
                                text.Span("Total Geral: ").Bold().FontSize(10);
                                text.Span(totalGeralValor.ToString("C2", culture)).Bold().FontSize(12).FontColor("#667eea");
                            });
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Página ").FontSize(8).FontColor("#a0aec0");
                        text.CurrentPageNumber().FontSize(8).FontColor("#a0aec0");
                        text.Span(" de ").FontSize(8).FontColor("#a0aec0");
                        text.TotalPages().FontSize(8).FontColor("#a0aec0");
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            string fileName = $"PreFatura_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        #region Private Helpers

        private void SetDefaultDateValues(ref DateTime? minDate, ref DateTime? maxDate)
        {
            if (!minDate.HasValue)
            {
                minDate = new DateTime(DateTime.Now.Year, 1, 1);
            }
            if (!maxDate.HasValue)
            {
                maxDate = DateTime.Now;
            }
        }

        private void PopulateViewData(DateTime? minDate, DateTime? maxDate, List<int> clientIds, int? attorneyId, string activityTypeId)
        {
            ViewData["minDate"] = minDate.Value.ToString("yyyy-MM-dd");
            ViewData["maxDate"] = maxDate.Value.ToString("yyyy-MM-dd");
            ViewData["clientIds"] = clientIds != null && clientIds.Any() ? string.Join(",", clientIds) : null;
            ViewData["attorneyId"] = attorneyId;
            ViewData["selectedActivityTypeId"] = activityTypeId;
        }

        private async Task PopulateViewBag()
        {
            ViewBag.Clients = await _clientService.FindAllAsync();
            ViewBag.Attorneys = await _attorneyService.FindAllAsync();
            ViewBag.Departments = await _departmentService.FindAllAsync();
            ViewBag.ActivityTypes = await _context.ActivityTypes
                .Where(at => at.IsActive)
                .OrderBy(at => at.DisplayOrder)
                .ToListAsync();

            Attorney usuario = _isessao.BuscarSessaoDoUsuario();
            ViewBag.LoggedUserId = usuario.Id;
            ViewBag.UserProfile = usuario.Perfil;
            ViewBag.CurrentUserPerfil = usuario.Perfil;
        }

        #endregion
    }
}

