namespace MaxiNet;

public interface IInitializable
{
    bool IsInitialized { get; }
    Result<Nothing> Initialize();
}

public abstract class Initializable : IInitializable
{
    private bool _isInitializing;
    public bool IsInitialized { get; private set; }


    public Result<Nothing> Initialize()
    {
        if (IsInitialized) return Res.Ok;

        if (_isInitializing)
            return new NegativeResult<Nothing>(new Oration("Initialization is already in progress on ?",
                [GetType().Name]));

        _isInitializing = true;


        Result<Nothing> initResult;
        try
        {
            initResult = PerformInitialization();
        }
        catch (Exception ex)
        {
            _isInitializing = false;
            return new ExceptionResult<Nothing>(ex,
                new Oration("An exception occurred during initialization: ?", [ex.Message]));
        }

        if (initResult is IFailure fail)
        {
            _isInitializing = false;
            return fail.Cast<Nothing>();
        }


        IsInitialized = true;
        _isInitializing = false;

        return Res.Ok;
    }

    protected abstract Result<Nothing> PerformInitialization();
}