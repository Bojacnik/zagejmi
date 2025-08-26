namespace Zagejmi.SharedKernel.Util;

public interface IMapper
{
    TTo? Map<TFrom, TTo>(TFrom entity);
}


