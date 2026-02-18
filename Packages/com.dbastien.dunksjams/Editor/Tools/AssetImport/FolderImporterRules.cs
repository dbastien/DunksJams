/*
public class FolderImporterRules : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;

        if (assetPath.StartsWith("Assets/Sprites"))
        {
            //textures in the sprites folder will automatically be set to the sprite type
            importer.textureType = TextureImporterType.Sprite;

            //automatically add to sprite sheet
            //this not only reduces draw call count
            //but prevents problems with non-power-of-two sizes with mip mapping and compression
            importer.spritePackingTag = "sprite";
        }

        if (assetPath.StartsWith("Assets/Textures"))
        {
        }
    }

    void OnPreprocessModel()
    {
        var importer = (ModelImporter)assetImporter;

        if (assetPath.StartsWith("Assets/Models"))
        {
            //don't automatically import materials - this ends up creating dupes
            //can import manually still by placing in another folder
            importer.importMaterials = false;
        }
    }

    void OnPreprocessAudio()
    {
    }
}
*/

