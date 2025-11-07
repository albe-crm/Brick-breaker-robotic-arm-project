using UnityEngine;

namespace NGIMU.Scripts
{
    internal static class NgimuMathUtils
    {
        public static Quaternion NgimuToUnityQuaternion(NgimuApi.Maths.Quaternion quaternion)
        {
            NgimuApi.Maths.Quaternion ngimuQuaternion = NgimuApi.Maths.Quaternion.Normalise(NgimuApi.Maths.Quaternion.Conjugate(quaternion));

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero /*Traslation*/, new Quaternion(ngimuQuaternion.X, ngimuQuaternion.Y, ngimuQuaternion.Z, ngimuQuaternion.W)/*Rotation*/, Vector3.one/*Scaling*/);

            Vector4 yColumn = matrix.GetColumn(1);
            Vector4 zColumn = matrix.GetColumn(2);

            matrix.SetColumn(1, zColumn);
            matrix.SetColumn(2, yColumn);

            Vector4 yRow = matrix.GetRow(1);
            Vector4 zRow = matrix.GetRow(2);

            matrix.SetRow(1, zRow);
            matrix.SetRow(2, yRow);

            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        }
        /* public static Vector3 NgimuToUnityVector(NgimuApi.Maths.Vector3 vector3)
        {
            NgimuApi.Maths.Vector3 ngimuVector3 = vector3;

            Vector3 prova = new Vector3(ngimuVector3.X, ngimuVector3.Y, ngimuVector3.Z);
            return prova;
        }*/
         public static Vector3 NgimuToUnityVector(NgimuApi.Maths.Vector3 EarthAcceleration)
        {
            NgimuApi.Maths.Vector3 ngimuVector = EarthAcceleration;

            Vector3 Earth = new Vector3(ngimuVector.X, ngimuVector.Y, ngimuVector.Z);
            return Earth;
        }
        public static Vector3 NgimuToUnityVector(NgimuApi.Maths.EulerAngles vector3)
        {
            NgimuApi.Maths.EulerAngles ngimuEulerAngles = vector3;

            Vector3 Angoli = new Vector3(ngimuEulerAngles.Roll, ngimuEulerAngles.Pitch, ngimuEulerAngles.Yaw);
            return Angoli;
        }
        /*public static Matrix4x4 NgimuToUnityRotationMatrix(NgimuApi.Maths.RotationMatrix matrice)
        {
            NgimuApi.Maths.RotationMatrix ngimuRotationMatrix = matrice;

            Matrix4x4 RotMat = new Matrix4x4(ngimuRotationMatrix.XX, ngimuRotationMatrix.XY, ngimuRotationMatrix.XZ, 0.0f,
                                             ngimuRotationMatrix.YX, ngimuRotationMatrix.YY, ngimuRotationMatrix.YZ, 0.0f,
                                             ngimuRotationMatrix.ZX, ngimuRotationMatrix.ZY, ngimuRotationMatrix.ZZ, 0.0f,
                                                               0.0f,                   0.0f,                   0.0f, 1.0f);
            return RotMat;
        }*/
    }
}