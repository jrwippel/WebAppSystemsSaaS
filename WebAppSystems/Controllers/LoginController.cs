using Microsoft.AspNetCore.Mvc;
using WebAppSystems.Helper;
using WebAppSystems.Models;
using WebAppSystems.Services;

namespace WebAppSystems.Controllers
{
    public class LoginController : Controller
    {
        private readonly AttorneyService _attorneyService;
        private readonly ISessao _sessao;
        private readonly IEmail _email;
        private readonly LoginAttemptService _loginAttemptService;

        public LoginController(AttorneyService attorneyService, ISessao sessao, IEmail email, LoginAttemptService loginAttemptService)
        {
            _attorneyService = attorneyService;
            _sessao = sessao;
            _email = email;
            _loginAttemptService = loginAttemptService;
        }

        public IActionResult Index()
        {
            string currentController = RouteData.Values["controller"]?.ToString();
            string currentAction = RouteData.Values["action"]?.ToString();

            if (currentController != "Login" || currentAction != "Index")
            {
                try
                {
                    // Tenta buscar a sessão do usuário e redireciona para a Home se estiver logado
                    if (_sessao.BuscarSessaoDoUsuario() != null)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (Sessao.SessionExpiredException)
                {
                    // Exibe uma mensagem amigável se a sessão expirou
                    TempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";
                }
            }
            return View();
        }

        public IActionResult TimeTracking()
        {
            return View();
        }

        public IActionResult RedefinirSenha()
        {
            return View();
        }

        public IActionResult Sair()
        {
            _sessao.RemoverSessaoDoUsuario();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]        
        public async Task<IActionResult> EnviarLinkParaRedefinirSenha(RedefinirSenhaModel redefinirSenhaModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuarioModel = _attorneyService.BuscarPorEmailLogin(redefinirSenhaModel.Email, redefinirSenhaModel.Login);

                    if (usuarioModel != null)
                    {
                        string novaSenha = usuarioModel.GerarNovaSenha();
                        string nomeUsuario = usuarioModel.Name?.Split(' ')[0] ?? usuarioModel.Login;
                        string mensagem = $"Sua nova senha temporária é: {novaSenha}";
                        string htmlEmail = $@"
<!DOCTYPE html>
<html lang='pt-br'>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#f4f6f9;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6f9;padding:40px 20px;'>
    <tr><td align='center'>
      <table width='600' cellpadding='0' cellspacing='0' style='max-width:600px;width:100%;'>

        <!-- Header -->
        <tr><td style='background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);border-radius:12px 12px 0 0;padding:40px 48px;text-align:center;'>
          <div style='width:64px;height:64px;background:rgba(255,255,255,0.2);border-radius:16px;display:inline-flex;align-items:center;justify-content:center;margin-bottom:16px;'>
            <span style='font-size:32px;'>🔐</span>
          </div>
          <h1 style='color:white;margin:0;font-size:26px;font-weight:700;letter-spacing:-0.5px;'>Time Tracker</h1>
          <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>Sistema de Controle Jurídico</p>
        </td></tr>

        <!-- Body -->
        <tr><td style='background:white;padding:48px;'>
          <h2 style='color:#1a202c;font-size:22px;font-weight:700;margin:0 0 8px;'>Olá, {nomeUsuario}!</h2>
          <p style='color:#4a5568;font-size:15px;line-height:1.6;margin:0 0 32px;'>
            Recebemos uma solicitação de redefinição de senha para sua conta. Sua nova senha temporária está abaixo.
          </p>

          <!-- Password Box -->
          <div style='background:#f7f8fc;border:2px dashed #667eea;border-radius:12px;padding:28px;text-align:center;margin-bottom:32px;'>
            <p style='color:#667eea;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:1px;margin:0 0 12px;'>Senha Temporária</p>
            <p style='color:#1a202c;font-size:32px;font-weight:700;letter-spacing:6px;margin:0;font-family:monospace;'>{novaSenha}</p>
          </div>

          <!-- Steps -->
          <p style='color:#4a5568;font-size:14px;font-weight:600;margin:0 0 16px;'>Próximos passos:</p>
          <table width='100%' cellpadding='0' cellspacing='0'>
            <tr>
              <td style='padding:10px 0;border-bottom:1px solid #e2e8f0;'>
                <span style='display:inline-block;width:24px;height:24px;background:#667eea;color:white;border-radius:50%;text-align:center;line-height:24px;font-size:12px;font-weight:700;margin-right:12px;'>1</span>
                <span style='color:#4a5568;font-size:14px;'>Acesse o sistema com esta senha temporária</span>
              </td>
            </tr>
            <tr>
              <td style='padding:10px 0;border-bottom:1px solid #e2e8f0;'>
                <span style='display:inline-block;width:24px;height:24px;background:#667eea;color:white;border-radius:50%;text-align:center;line-height:24px;font-size:12px;font-weight:700;margin-right:12px;'>2</span>
                <span style='color:#4a5568;font-size:14px;'>Você será redirecionado para criar uma nova senha</span>
              </td>
            </tr>
            <tr>
              <td style='padding:10px 0;'>
                <span style='display:inline-block;width:24px;height:24px;background:#667eea;color:white;border-radius:50%;text-align:center;line-height:24px;font-size:12px;font-weight:700;margin-right:12px;'>3</span>
                <span style='color:#4a5568;font-size:14px;'>Defina uma senha pessoal e segura</span>
              </td>
            </tr>
          </table>

          <!-- Warning -->
          <div style='background:#fffbeb;border-left:4px solid #f6ad55;border-radius:0 8px 8px 0;padding:16px 20px;margin-top:32px;'>
            <p style='color:#744210;font-size:13px;margin:0;'>
              ⚠️ <strong>Atenção:</strong> Se você não solicitou a redefinição de senha, ignore este email. Sua senha atual permanece inalterada.
            </p>
          </div>
        </td></tr>

        <!-- Footer -->
        <tr><td style='background:#f7f8fc;border-radius:0 0 12px 12px;padding:24px 48px;text-align:center;border-top:1px solid #e2e8f0;'>
          <p style='color:#a0aec0;font-size:12px;margin:0 0 4px;'>Este é um email automático, não responda.</p>
          <p style='color:#667eea;font-size:12px;font-weight:600;margin:0;'>JRWIT.COM · Smart Solutions For Your Business</p>
        </td></tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
                        bool emailEnviado = await _email.EnviarAsync(usuarioModel.Email, "Time Tracker — Sua nova senha temporária", mensagem, htmlBody: htmlEmail);
                        //bool emailEnviado = await _email.EnviarAsync(usuarioModel.Email, "Stradale - Sistema de Controle Recolhas - Nova Senha", mensagem);

                        if (emailEnviado)
                        {
                            usuarioModel.MustChangePassword = true;
                            _attorneyService.AtualizarSenha(usuarioModel);
                            TempData["MensagemSucesso"] = "Enviamos para o seu email cadastrado uma nova senha.";
                        }
                        else
                        {
                            TempData["MensagemErro"] = "Não conseguimos enviar o email. Tente novamente.";
                        }
                        return RedirectToAction("Index", "Login");
                    }
                    TempData["MensagemErro"] = "Não conseguimos redefinir sua senha. Dados informados inválidos.";
                }

                return View("Index");
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não conseguimos redefinir sua senha, tente novamente. Detalhes do erro: {erro.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Entrar(LoginModel loginModel)
        {
            try
            {
                // Verifica se o estado do modelo é válido
                if (ModelState.IsValid)
                {
                    // Verifica se o login está bloqueado por tentativas excessivas
                    // Usa o email como identificador para o bloqueio
                    if (_loginAttemptService.IsLockedOut(loginModel.Login))
                    {
                        var lockoutTime = _loginAttemptService.GetLockoutTimeRemaining(loginModel.Login);
                        if (lockoutTime.HasValue)
                        {
                            TempData["MensagemErro"] = $"Conta temporariamente bloqueada devido a múltiplas tentativas de login. Tente novamente em {lockoutTime.Value.Minutes} minutos e {lockoutTime.Value.Seconds} segundos.";
                        }
                        else
                        {
                            TempData["MensagemErro"] = "Conta temporariamente bloqueada devido a múltiplas tentativas de login.";
                        }
                        return View("Index");
                    }

                    // Busca usuário por email ao invés de login
                    var usuario = _attorneyService.FindByEmailAsync(loginModel.Login);

                    if (usuario != null)
                    {
                        if (usuario.Inativo)
                        {
                            TempData["MensagemErro"] = "Usuário inativo. Contate o administrador para mais informações.";
                            return View("Index");
                        }

                        if (usuario.ValidaSenha(loginModel.Senha))
                        {
                            // Login bem-sucedido - reseta tentativas
                            _loginAttemptService.ResetAttempts(loginModel.Login);

                            // Migração automática: se ainda usa SHA-1, atualiza para PBKDF2
                            if (!usuario.Password.Contains(':'))
                            {
                                usuario.SetNovaSenha(loginModel.Senha);
                                _attorneyService.AtualizarSenha(usuario);
                            }
                            
                            _sessao.CriarSessaoDoUsuario(usuario);
                            
                            // Salvar TenantId na sessão para isolamento de dados
                            HttpContext.Session.SetInt32("TenantId", usuario.TenantId);
                            
                            // Força troca de senha se foi gerada senha temporária
                            if (usuario.MustChangePassword)
                            {
                                TempData["MensagemAviso"] = "Você está usando uma senha temporária. Por favor, defina uma nova senha.";
                                return RedirectToAction("Index", "AlterarSenha");
                            }
                            
                            return RedirectToAction("Index", "Home");
                        }

                        // Senha inválida - registra tentativa falha
                        _loginAttemptService.RecordFailedAttempt(loginModel.Login);
                        var remainingAttempts = _loginAttemptService.GetRemainingAttempts(loginModel.Login);
                        
                        if (remainingAttempts > 0)
                        {
                            TempData["MensagemErro"] = $"Senha inválida. Você tem {remainingAttempts} tentativa(s) restante(s).";
                        }
                        else
                        {
                            TempData["MensagemErro"] = "Senha inválida. Conta bloqueada por 15 minutos devido a múltiplas tentativas.";
                        }
                    }
                    else
                    {
                        // Usuário não encontrado - registra tentativa falha
                        _loginAttemptService.RecordFailedAttempt(loginModel.Login);
                        var remainingAttempts = _loginAttemptService.GetRemainingAttempts(loginModel.Login);
                        
                        if (remainingAttempts > 0)
                        {
                            TempData["MensagemErro"] = $"Email e/ou senha inválido(s). Você tem {remainingAttempts} tentativa(s) restante(s).";
                        }
                        else
                        {
                            TempData["MensagemErro"] = "Email e/ou senha inválido(s). Conta bloqueada por 15 minutos devido a múltiplas tentativas.";
                        }
                    }
                }
                else
                {
                    // Remove mensagens de erro desnecessárias caso a validação inicial falhe
                    TempData["MensagemErro"] = null;
                }

                return View("Index");
            }
            catch (Sessao.SessionExpiredException)
            {
                TempData["MensagemErro"] = "A sessão expirou. Por favor, faça login novamente.";
                return RedirectToAction("Index");
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não conseguimos realizar o seu login. Mais detalhes no erro: {erro.Message}";
                return RedirectToAction("Index");
            }
        }


    }
}
