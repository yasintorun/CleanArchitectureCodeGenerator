using EnvDTE;

namespace CleanArchitectureCodeGenerator.Models
{
    public class CreateFileParameters
    {
        public ProjectItem ProjectItem { get; set; }
        public string ProjectTemplate { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string ProjectName { get; set; }
    }
}
