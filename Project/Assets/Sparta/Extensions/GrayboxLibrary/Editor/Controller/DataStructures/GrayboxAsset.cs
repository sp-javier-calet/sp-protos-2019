using UnityEngine;
using System;
namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxAsset
    {
        public int id;
        public string name, packagePath, thumbnailPath, animatedThumbnailPath, mainAssetPath;
        public GrayboxAssetCategory category;
        public Texture thumbnail;
        public DateTime creation_date;

        public GrayboxAsset()
        {
            this.id = -1;
            this.name = "";
            this.category = GrayboxAssetCategory.Buildings;
            this.mainAssetPath = "";
            this.packagePath = "";
            this.thumbnailPath = "";
            this.animatedThumbnailPath = "";
            this.thumbnail = null;
            this.creation_date = new DateTime();
        }

        public GrayboxAsset(int id, string name, GrayboxAssetCategory category, string mainAssetPath, string packagePath, string thumbnailPath, string animatedThumbnailPath, Texture thumbnail, DateTime creation_date)
        {
            this.id = id;
            this.name = name;
            this.category = category;
            this.mainAssetPath = mainAssetPath;
            this.packagePath = packagePath;
            this.thumbnailPath = thumbnailPath;
            this.animatedThumbnailPath = animatedThumbnailPath;
            this.thumbnail = thumbnail;
            this.creation_date = creation_date;
        }

    }

    public enum GrayboxAssetCategory { Buildings, Props, Fx, Characters, Vehicles, UI };
}