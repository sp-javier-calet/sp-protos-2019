using UnityEngine;
using System;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxAsset
    {
        public int Id;
        public string Name, PackagePath, ThumbnailPath, AnimatedThumbnailPath, MainAssetPath;
        public GrayboxAssetCategory Category;
        public Texture Thumbnail;
        public DateTime CreationDate;

        public GrayboxAsset()
        {
            this.Id = -1;
            this.Name = "";
            this.Category = GrayboxAssetCategory.Buildings;
            this.MainAssetPath = "";
            this.PackagePath = "";
            this.ThumbnailPath = "";
            this.AnimatedThumbnailPath = "";
            this.Thumbnail = null;
            this.CreationDate = new DateTime();
        }

        public GrayboxAsset(int id, string name, GrayboxAssetCategory category, string mainAssetPath, string packagePath, string thumbnailPath, string animatedThumbnailPath, Texture thumbnail, DateTime creationDate)
        {
            this.Id = id;
            this.Name = name;
            this.Category = category;
            this.MainAssetPath = mainAssetPath;
            this.PackagePath = packagePath;
            this.ThumbnailPath = thumbnailPath;
            this.AnimatedThumbnailPath = animatedThumbnailPath;
            this.Thumbnail = thumbnail;
            this.CreationDate = creationDate;
        }
    }
}