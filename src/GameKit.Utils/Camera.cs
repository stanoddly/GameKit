using System.Numerics;

namespace GameKit.Utils;

public abstract class Camera
{
    private Vector3 _position;
    private Quaternion _rotation;
    private float _nearPlane;
    private float _farPlane;
    
    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewProjectionMatrix;
    
    private bool _viewDirty = true;
    private bool _projectionDirty = true;

    public Vector3 Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                _viewDirty = true;
            }
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                _viewDirty = true;
            }
        }
    }

    public float NearPlane
    {
        get => _nearPlane;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_nearPlane != value)
            {
                _nearPlane = value;
                _projectionDirty = true;
            }
        }
    }

    public float FarPlane
    {
        get => _farPlane;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_farPlane != value)
            {
                _farPlane = value;
                _projectionDirty = true;
            }
        }
    }

    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (_viewDirty)
            {
                _viewMatrix = ComputeViewMatrix();
                _viewDirty = false;
            }
            return _viewMatrix;
        }
    }

    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (_projectionDirty)
            {
                _projectionMatrix = ComputeProjectionMatrix();
                _projectionDirty = false;
            }
            return _projectionMatrix;
        }
    }
    
    public Matrix4x4 ViewProjectionMatrix
    {
        get
        {
            if (_projectionDirty || _viewDirty)
            {
                // flags are cleared by the getters
                _viewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            }

            return _viewProjectionMatrix;
        }
    }

    protected void MarkProjectionDirty() => _projectionDirty = true;

    private Matrix4x4 ComputeViewMatrix()
    {
        Vector3 forward = Vector3.Transform(-Vector3.UnitZ, _rotation);
        Vector3 up = Vector3.Transform(Vector3.UnitY, _rotation);
        
        return Matrix4x4.CreateLookAtLeftHanded(_position, _position + forward, up);
    }

    protected abstract Matrix4x4 ComputeProjectionMatrix();
}

public class OrthographicCamera : Camera
{
    private float _width;
    private float _height;

    public float Width
    {
        get => _width;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_width != value)
            {
                _width = value;
                MarkProjectionDirty();
            }
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_height != value)
            {
                _height = value;
                MarkProjectionDirty();
            }
        }
    }

    protected override Matrix4x4 ComputeProjectionMatrix()
    {
        Matrix4x4 projectionMatrix = Matrix4x4.CreateOrthographicLeftHanded(_width, _height, NearPlane, FarPlane);

        Matrix4x4 reverseDepthMatrix = new(
            1f,  0f,   0f, 0f,
            0f,  1f,   0f, 0f,
            0f,  0f,  -1f, 0f,
            0f,  0f,   1f, 1f
        );
        
        return Matrix4x4.Multiply(projectionMatrix, reverseDepthMatrix);
    }
}

public class PerspectiveCamera : Camera
{
    private float _fieldOfView;
    private float _aspectRatio;

    public float FieldOfView
    {
        get => _fieldOfView;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_fieldOfView != value)
            {
                _fieldOfView = value;
                MarkProjectionDirty();
            }
        }
    }

    public float AspectRatio
    {
        get => _aspectRatio;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_aspectRatio != value)
            {
                _aspectRatio = value;
                MarkProjectionDirty();
            }
        }
    }

    protected override Matrix4x4 ComputeProjectionMatrix()
    {
        Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(_fieldOfView, _aspectRatio, NearPlane, FarPlane);
        
        Matrix4x4 reverseDepthMatrix =  new(
            1f,  0f,   0f, 0f,
            0f,  1f,   0f, 0f,
            0f,  0f,  -1f, 0f,
            0f,  0f,   1f, 1f
        );
        
        return Matrix4x4.Multiply(projectionMatrix, reverseDepthMatrix);
    }
}