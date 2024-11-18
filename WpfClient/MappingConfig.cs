using AutoMapper;
using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfClient.Models;

namespace WpfClient
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                // Employee mappings
                config.CreateMap<Employee, EmployeeModel>();
                config.CreateMap<EmployeeModel, Employee>();

                // Vacation mappings
                config.CreateMap<Vacation, VacationModel>();
                config.CreateMap<VacationModel, Vacation>();
            });

            return mappingConfig;
        }
    }
}
