namespace LibraryMS.Core.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        string Render(string templateName, Dictionary<string, string> values);
    }
}
