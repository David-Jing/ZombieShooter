using UnityEngine;
using System;
using System.Collections.Generic;
using PicaVoxel;

namespace Mikkeo.Extensions
{
    public static class PicaVoxelExtensions
    {

        static Voxel nullVox;

        static PicaVoxelPoint[] axisOffsets = {
         new PicaVoxelPoint( 1, 0, 0),
         new PicaVoxelPoint(-1, 0, 0),
         new PicaVoxelPoint( 0, 1, 0),
         new PicaVoxelPoint( 0,-1, 0),
         new PicaVoxelPoint( 0, 0, 1),
         new PicaVoxelPoint( 0, 0,-1)
    };


        public static PicaVoxelPoint Offset(this PicaVoxelPoint c1, PicaVoxelPoint c2)
        {
            return new PicaVoxelPoint(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z);
        }


        public static PicaVoxelPoint Offset(this PicaVoxelPoint c1, int x, int y, int z)
        {
            return new PicaVoxelPoint(c1.X + x, c1.Y + y, c1.Z + z);
        }

        public static bool IsInvalidGridPos(this Volume volume, PicaVoxelPoint pt)
        {
            return pt.X < 0 || pt.Y < 0 || pt.Z < 0
                || pt.X >= volume.XSize || pt.Y >= volume.YSize || pt.Z >= volume.ZSize;
        }

        public static Voxel GetVoxel(this Volume volume, PicaVoxelPoint pt)
        {
            if (volume.IsInvalidGridPos(pt))
                return nullVox;

            int index = pt.X + volume.XSize * (pt.Y + volume.YSize * pt.Z);

            return volume.GetCurrentFrame().Voxels[index];
        }

        public static bool IsVoxelEmpty(this Volume volume, PicaVoxelPoint pt)
        {
            if (volume.IsInvalidGridPos(pt))
                return false;

            return volume.GetVoxel(pt).Active;
        }

        public static IEnumerable<PicaVoxelPoint> FindValues(this Volume volume, int value)
        {

            var voxelArray = volume.GetCurrentFrame().Voxels;

            for (int i = 0; i < voxelArray.Length; ++i)
            {
                if (voxelArray[i].Value == value)
                {
                    var z = i / (volume.XSize * volume.YSize);
                    var r = i % (volume.XSize * volume.YSize);
                    var y = r / volume.XSize;
                    var x = r % volume.XSize;
                    yield return new PicaVoxelPoint(x, y, z);
                }
            }
        }


        public static PicaVoxelPoint FindValue(this Volume volume, int value)
        {
            var voxelArray = volume.GetCurrentFrame().Voxels;

            for (int i = 0; i < voxelArray.Length; ++i)
            {
                if (voxelArray[i].Value == value)
                {
                    var z = i / (volume.XSize * volume.YSize);
                    var r = i % (volume.XSize * volume.YSize);
                    var y = r / volume.XSize;
                    var x = r % volume.XSize;
                    return new PicaVoxelPoint(x, y, z);
                }
            }

            return new PicaVoxelPoint(-1, -1, -1);
        }

        public static PicaVoxelPoint FindEmptyAdjacent(this Volume volume, PicaVoxelPoint pt)
        {
            var voxelArray = volume.GetCurrentFrame().Voxels;

            foreach (var ao in axisOffsets)
            {
                var newPos = pt.Offset(ao);
                if (!volume.GetVoxel(newPos).Active)
                {
                    return newPos;
                }
            }

            return new PicaVoxelPoint(-1, -1, -1);
        }

        public static Vector3 GetVoxelWorldPos(this Volume volume, PicaVoxelPoint pt)
        {
            return volume.GetVoxelWorldPosition(pt.X, pt.Y, pt.Z);
        }
    }

}