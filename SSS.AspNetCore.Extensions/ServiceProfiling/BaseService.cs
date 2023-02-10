using AutoMapper;
using Microsoft.Extensions.Logging;
using SSS.AspNetCore.Extensions.Exceptions;
using System;

namespace SSS.AspNetCore.Extensions.ServiceProfiling
{
    public abstract class BaseService<TService>
    {

        protected IMapper _mapper;
        protected ILogger<TService> _logger;

        public BaseService(IMapper mapper, ILoggerFactory loggerFactory)
        {
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<TService>();
        }

        protected static void BadRequest(string message)
        {
            throw new BadRequestException(message);
        }

        protected static void Unauthorized()
        {
            throw new UnauthorizedAccessException();
        }

        protected TEntity MapToEntity<TEntity, TDto>(TDto dto)
        {
            return _mapper.Map<TEntity>(dto);
        }

        protected TEntity MapToEntity<TEntity, TDto>(TDto dto, TEntity entity)
        {
            return _mapper.Map(dto, entity);
        }

        protected TDto MapToDto<TEntity, TDto>(TEntity entity)
        {
            return _mapper.Map<TDto>(entity);
        }
    }
}
