namespace MediatrTests.Simple;

public interface IAssertService<T>
{
    void Process(T item);
}