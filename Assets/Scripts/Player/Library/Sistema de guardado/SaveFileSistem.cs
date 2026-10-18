using UnityEngine; 
using RMC.SaveFile.EventLibrary; 
using System.IO;
using System.Collections.Generic;
using System;

///<permission cref="Licencia de uso sobre El siguiente sistema de guardado"
/// El siguiente sistema de guardado pertenece a Uriel Antonio Gomez Madriz con sede en costa rica en su totalidad
/// Esta libreria lo unico hace es serialziar un archivo de guardado, no tiene absolutamente nada mas que hacer
/// Para mas informacion consulte "Proyect Reveria" en Github (Si Github dejo de existir puede tambien mandar un correo a urielgomezmadriz@gmail.com)
/// </Fin>

namespace RMC.SaveFile.SaveSerialice 
{

    class Path_List
    {
        public static readonly string RotPath = Application.persistentDataPath;
        
        //directory 
        public static readonly string DirectoryName = "SaveFiles"; 
        public static readonly string Directory_PathSaveFile = Path.Combine(RotPath,DirectoryName);

        //SaveFile

        public static readonly string SaveFile_Path = Path.Combine(Directory_PathSaveFile, "SaveFile.json");


    }

    class FileGen_Methods
    {
        public static void Create_files()
        {
            Gen_File_Requirements.GenDirectory(Path_List.Directory_PathSaveFile);
            Gen_File_Requirements.GenFile(Path_List.SaveFile_Path);
        }
        
    }
    class SerialiceGameData 
    {
        public static void Serialize_GameData<SaveData>(SaveData Game_data, string Save_FilePath)
        {
            string json = JsonUtility.ToJson(Game_data, true); 
            File.WriteAllText(Save_FilePath, json);
            EventLibrary.EventLibrary.Execute_GameSaveLoading(); 
        }

        public static void Deserialice_GameDAta<SaveData> (string Path, Action<SaveData> onsaveFound = null, Action onSaveNotFound = null)
        {
            if (File.Exists(Path_List.SaveFile_Path))
            {
                EventLibrary.EventLibrary.Execute_GameendstartLoading();
                string json = File.ReadAllText(Path);
                SaveData Game_data = JsonUtility.FromJson<SaveData>(json); 
                EventLibrary.EventLibrary.Execute_GameendendLoading();

                onsaveFound.Invoke(Game_data); 
            }
            else
            {
                onSaveNotFound?.Invoke(); 
            }
        }

        public static void Erase_serialiceData()
        {
            File.Create(Path_List.SaveFile_Path).Close(); 
        }
    }

    class Gen_File_Requirements
    {
        
        public static void GenDirectory (string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (DirectoryNotFoundException FailOnGenDirectory)
            {
                Debug.Log($"[D/] {FailOnGenDirectory}");
            }
        }

        public static void GenFile (string path)
        {
            try
            {
                File.Create(path).Close(); 
            } 
            catch (FileNotFoundException FailonGenFile)
            {
                Debug.Log($"[F/] {FailonGenFile}");
            }
        }

    }

} 
