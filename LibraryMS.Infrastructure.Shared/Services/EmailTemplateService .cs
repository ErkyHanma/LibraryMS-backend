using LibraryMS.Core.Application.Interfaces;

namespace LibraryMS.Infrastructure.Shared.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string Render(string templateName, Dictionary<string, string> values)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Templates", "Email", templateName);
            var html = File.ReadAllText(path);

            foreach (var (key, value) in values)
                html = html.Replace($"{{{{{key}}}}}", value);

            return html;
        }
    }
}
