namespace DrawOutApp.Server
{
    public readonly struct Result<TValue, TError>
    {
        public bool IsError { get; }
        public bool IsSuccess => !IsError;

        public TValue Data
        {
            get
            {
                return _value ?? default!;
            }
        }

        public TError Error
        {
            get
            {
                return _error ?? default!;
            }
        }

        private readonly TValue? _value;
        private readonly TError? _error;

        private Result(TValue value)
        {
            IsError = false;
            _value = value;
            _error = default;
        }

        private Result(TError error)
        {
            IsError = true;
            _error = error;
            _value = default;
        }

        public static implicit operator Result<TValue, TError>(TValue value) => new(value);

        public static implicit operator Result<TValue, TError>(TError error) => new(error);

        public void Deconstruct(out bool isError, out TValue? value, out TError? error)
        {
            isError = IsError;
            value = _value;
            error = _error;
        }
    }
}
