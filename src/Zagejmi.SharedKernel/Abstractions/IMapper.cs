namespace Zagejmi.SharedKernel.Abstractions;

public interface IMapper
{
    public TTo Map<TFrom, TTo>(TFrom entity);
}