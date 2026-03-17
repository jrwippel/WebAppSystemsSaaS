namespace ClockTrack.Models
{
    public class DashboardKpis
    {
        public double HorasHoje { get; set; }
        public double HorasOntem { get; set; }
        public double HorasMes { get; set; }
        public double HorasMesPassado { get; set; }
        public int RegistrosHoje { get; set; }
        public int ClientesAtivos { get; set; }

        public string HorasHojeFormatted => FormatHours(HorasHoje);
        public string HorasMesFormatted => FormatHours(HorasMes);

        public double VariacaoHoje => HorasOntem > 0 ? Math.Round((HorasHoje - HorasOntem) / HorasOntem * 100, 0) : 0;
        public double VariacaoMes => HorasMesPassado > 0 ? Math.Round((HorasMes - HorasMesPassado) / HorasMesPassado * 100, 0) : 0;

        private static string FormatHours(double h)
        {
            int hours = (int)h;
            int minutes = (int)((h - hours) * 60);
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
    }
}
