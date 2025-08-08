using UnityEditor;

public class ImportSettings : AssetPostprocessor
{
    public void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        //modelImporter.bakeAxisConversion = true;
        modelImporter.importVisibility = false;
        modelImporter.importLights = false;
        modelImporter.importCameras = false;
    }
}
    
