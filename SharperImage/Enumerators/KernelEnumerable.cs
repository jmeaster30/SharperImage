using System.Collections;

namespace SharperImage.Enumerators;

public enum KernelEdgeMode
{
    SHRINK,
    WRAP,
    EXTEND,
    MIRROR,
    BLACK,
    CLEAR,
}

public class KernelEnumerable : IPixelEnumerable
{
    private KernelEnumerator _enumerator;
    
    public KernelEnumerable(IPixelEnumerable enumerable, uint kernelWidth, uint kernelHeight,
        Func<Pixel[,], Color> kernelFunction, KernelEdgeMode edgeMode = KernelEdgeMode.EXTEND)
    {
        var kernelOffsetX = (uint)System.Math.Floor(kernelWidth / 2.0);
        var kernelOffsetY = (uint)System.Math.Floor(kernelHeight / 2.0);
        _enumerator = new KernelEnumerator(enumerable, kernelWidth, kernelHeight, kernelOffsetX, kernelOffsetY, kernelFunction, edgeMode);
    }

    public KernelEnumerable(IPixelEnumerable enumerable, uint kernelWidth, uint kernelHeight,
        uint kernelOffsetX, uint kernelOffsetY, Func<Pixel[,], Color> kernelFunction, KernelEdgeMode edgeMode = KernelEdgeMode.EXTEND)
    {
        _enumerator = new KernelEnumerator(enumerable, kernelWidth, kernelHeight, kernelOffsetX, kernelOffsetY, kernelFunction, edgeMode);
    }
    
    public IEnumerator<Pixel> GetEnumerator()
    {
        return _enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _enumerator.Count;
    public uint GetWidth() => _enumerator.GetWidth();
    public uint GetHeight() => _enumerator.GetHeight();

    public Pixel this[int index]
    {
        get
        {
            _enumerator.SetIndex((uint)index);
            return _enumerator.Current;
        }
    }

    public Pixel this[uint x, uint y]
    {
        get
        {
            _enumerator.SetX(x);
            _enumerator.SetY(y);
            return _enumerator.Current;
        }
    }
}

public class KernelEnumerator : IPixelEnumerator
{
    private readonly IPixelEnumerable _enumerable;
    private readonly Func<Pixel[,], Color> _kernelFunction;
    private readonly uint _kernelWidth;
    private readonly uint _kernelHeight;
    private readonly uint _kernelOffsetX;
    private readonly uint _kernelOffsetY;
    private readonly KernelEdgeMode _mode;
    private readonly uint _width;
    private readonly uint _height;
    
    private uint _x;
    private uint _y;

    public KernelEnumerator(IPixelEnumerable enumerable, uint kernelWidth, uint kernelHeight,
        uint kernelOffsetX, uint kernelOffsetY, Func<Pixel[,], Color> kernelFunction, KernelEdgeMode edgeMode)
    {
        _enumerable = enumerable;
        _kernelFunction = kernelFunction;
        _kernelWidth = kernelWidth;
        _kernelHeight = kernelHeight;
        _kernelOffsetX = kernelOffsetX;
        _kernelOffsetY = kernelOffsetY;
        _mode = edgeMode;
        _width = _enumerable.GetWidth() - edgeMode switch
        {
            KernelEdgeMode.SHRINK => _kernelWidth - 1,
            _ => 0
        };
        _height = _enumerable.GetHeight() - edgeMode switch
        {
            KernelEdgeMode.SHRINK => _kernelHeight - 1,
            _ => 0
        };
    }
    
    public bool MoveNext()
    {
        _x += 1;
        if (_x >= _width)
        {
            _x = 0;
            _y += 1;
        }

        return _y < _height;
    }

    public void Reset()
    {
        _x = 0;
        _y = 0;
    }

    public Pixel Current
    {
        get
        {
            var adjX = _mode switch { KernelEdgeMode.SHRINK => _x + _kernelOffsetX, _ => _x };
            var adjY = _mode switch { KernelEdgeMode.SHRINK => _y + _kernelOffsetY, _ => _y };

            var kernel = new Pixel[_kernelWidth, _kernelHeight];

            for (var kx = 0; kx < _kernelWidth; kx++)
            {
                for (var ky = 0; ky < _kernelHeight; ky++)
                {
                    var adjKx = (long)adjX - _kernelOffsetX + kx;
                    var adjKy = (long)adjY - _kernelOffsetY + ky;
                    
                    kernel[kx, ky] = _mode switch
                    {
                        KernelEdgeMode.SHRINK => _enumerable[(uint)adjKx, (uint)adjKy],
                        KernelEdgeMode.WRAP => _enumerable[(uint)((adjKx + _enumerable.GetWidth()) % _enumerable.GetWidth()), (uint)((adjKy + _enumerable.GetHeight()) % _enumerable.GetHeight())],
                        KernelEdgeMode.EXTEND => _enumerable[Clamp(adjKx, _enumerable.GetWidth() - 1), Clamp(adjKy, _enumerable.GetHeight() - 1)],
                        KernelEdgeMode.BLACK => GetColorPixel(adjKx, adjKy, Color.Black),
                        KernelEdgeMode.CLEAR => GetColorPixel(adjKx, adjKy, Color.Clear),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }

            return new Pixel(_x, _y, _kernelFunction(kernel));
        }
    }

    private Pixel GetColorPixel(long adjKx, long adjKy, Color color)
    {
        if (adjKx < 0 || adjKx >= _enumerable.GetWidth() || adjKy < 0 || adjKy >= _enumerable.GetHeight())
        {
            return new Pixel(Clamp(adjKx, _enumerable.GetWidth() - 1), Clamp(adjKy, _enumerable.GetHeight() - 1), color);
        }

        return _enumerable[(uint)adjKx, (uint)adjKy];
    }

    private static uint Clamp(long adjK, uint max)
    {
        if (adjK < 0)
            return 0;
        if (adjK > max)
            return max;
        return (uint)adjK;
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        
        GC.SuppressFinalize(this);
    }

    public int Count => (int)(_width * _height);
    public uint GetWidth() => _width;
    public uint GetHeight() => _height;

    public void SetIndex(uint index)
    {
        _x = index % _width;
        _y = index / _height;
    }

    public void SetX(uint x)
    {
        _x = x;
    }

    public void SetY(uint y)
    {
        _y = y;
    }
}