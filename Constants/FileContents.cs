using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitectureCodeGenerator.Constants
{
    public class FileContents
    {
        public const string ApplicationIRepository = @"
        using LMS.Domain.Models;

        namespace LMS.Application.Abstractions.Repositories
        {
            public interface I${ModelName}Repository
            {

            }
        }";

        public const string ApplicationIService = @"
        using LMS.Domain.Models;

        namespace LMS.Application.Abstractions.Services
        {
            public interface I${ModelName}Service : IBaseService
            {

            }
        }";

        public const string ApplicationManager = @"
        using LMS.Application.Abstractions.Repositories;
        using LMS.Application.Abstractions.Services;
        using LMS.Domain.Models;
        using Microsoft.Extensions.Logging;

        namespace LMS.Application.Managers
        {
            public class ${ModelName}Manager
            {
                private readonly ILogger<${ModelName}Manager> _logger;
                private readonly I${ModelName}Repository _${camelModelName}Repository;

                public ${ModelName}Manager(ILogger<${ModelName}Manager> logger, I${ModelName}Repository ${camelModelName}Repository)
                {
                    _logger = logger;
                    _${camelModelName}Repository = ${camelModelName}Repository;
                }
            }
        }";


        public const string InfraStructureRepository = @"
        using LMS.Application.Abstractions.Repositories;
        using LMS.Domain.Models;
        using LMS.Infrastructure.Contexts;
        using LMS.Infrastructure.DBTable;

        namespace LMS.Infrastructure.Repositories
        {
            public class ${ModelName}Repository : I${ModelName}Repository
            {
                private readonly DijidemiContext _context;

                public ${ModelName}Repository(DijidemiContext context)
                {
                    _context = context;
                }
            }
        }";


    }
}
