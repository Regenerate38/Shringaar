using UnityEngine;
using UnityEngine.UI; 
using System;

public class ImagePicker : MonoBehaviour
{
    public Image targetImage; 

    public void PickImage()
    {
        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        if (permission == NativeGallery.Permission.Granted)
        {
            OpenGallery();
        }
        else if (permission == NativeGallery.Permission.ShouldAsk)
        {
            // Request permission
            NativeGallery.RequestPermissionAsync((result) =>
            {
                if (result == NativeGallery.Permission.Granted)
                {
                    OpenGallery();
                }
            }, NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        }
    }

    private void OpenGallery()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture != null)
                {
                    targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
            }
            else
            {
                Debug.Log("No image selected");
            }
        }, "Select an image", "image/*");
    }
}