using UnityEngine; 
using RMC.Player_Interaction.ThePlayerGeneralManager;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;


///<permission cref="Licencia de uso sobre El siguiente sistema de guardado"
/// El siguiente sistema de guardado pertenece a Uriel Antonio Gomez Madriz junto a la Rosary Musical composition con sede en costa rica en su totalidad
/// Esta libreria lo unico hace es serialziar un archivo de guardado, no tiene absolutamente nada mas que hacer
/// Para mas informacion consulte "Proyect Reveria" en Github (Si Github dejo de existir puede tambien mandar un correo a urielgomezmadriz@gmail.com)
/// Dicho codigo es de uso libre.
/// </Fin>
/// <Nota>
/// Nunca hacer dos archivos de guardado a la ves
/// 
/// /B = backup
/// /D = directoru
/// /F = file
/// <fin de la nota>

namespace RMC.SaveFile.SaveSerialice 
{
    #region  Path_List
    class Path_List
    {
        public static string RotPath;
        
        //directory 
        public static readonly string DirectoryName = "SaveFiles"; 
        public static string Directory_PathSaveFile;
        //SaveFile
        public static string GustomName = "SaveFile";  //Recordar implementar para poner los gustom name y para poner la opcion de si sobre- escribir o no.
        public static string DefaultName = "SaveFile";
        public static string DefaultSaveFile_Path;

        public static string SaveFile_Path;

        public static void InitiatePath()
        {
            RotPath = Application.persistentDataPath;
            Directory_PathSaveFile = Path.Combine(RotPath, DirectoryName);
            DefaultSaveFile_Path = Path.Combine(Directory_PathSaveFile, DefaultName);
            SaveFile_Path = Path.Combine(Directory_PathSaveFile, GustomName);
        }

    }
    #endregion
    #region  File Creation
    class FileGen_Methods
    {
        public static void Create_files(string path)
        {
            Path_List.SaveFile_Path = Path.Combine(Path_List.Directory_PathSaveFile,path + ".json");
            Gen_File_Requirements.GenFile(Path_List.SaveFile_Path);
        }
        
    }
        class Gen_File_Requirements
    {
        public static bool OverrideFile = false;
        public static void GenDirectory ()
        {
            try
            {
                Directory.CreateDirectory(Path_List.Directory_PathSaveFile);
            }
            catch (DirectoryNotFoundException FailOnGenDirectory)
            {
                Debug.Log($"[D/] {FailOnGenDirectory}");
            }
            catch (Exception FailOnGenDirectoryGeneral)
            {
                Debug.Log($"[D/] {FailOnGenDirectoryGeneral}");
            }
        }

        public static void GenFile (string path)
        {
            if (!Directory.Exists(Path_List.Directory_PathSaveFile))
            {
                return;
            }
            try
            {
                if (File.Exists(path) && OverrideFile == false)
                {
                    int Adition = UnityEngine.Random.Range(1,42891741);
                    string tempPath = Path_List.GustomName + Adition.ToString() + ".json";
                    while (File.Exists(Path.Combine(Path_List.Directory_PathSaveFile,tempPath)))
                    {
                        int Adition_onFix = UnityEngine.Random.Range(1,42891741);
                        string Path_Fixing = tempPath.Replace(".json","");
                        Path_Fixing = Path_Fixing + Adition_onFix.ToString() + ".json";
                        tempPath = Path_Fixing;
                    }
                    Path_List.SaveFile_Path = Path.Combine(Path_List.Directory_PathSaveFile,tempPath);
                    path =  Path_List.SaveFile_Path;
                    File.Create(path).Close(); 
                }else
                {
                    File.Create(path).Close(); 
                }
            } 
            catch (FileNotFoundException FailonGenFile)
            {
                Debug.Log($"[F/] {FailonGenFile}");
            }
            catch (Exception FailonGenFileGeneral)
            {
                Debug.Log($"[F/] {FailonGenFileGeneral}");
            }
        }

    }
    #endregion
    #region  Versioning
    public interface IVersionedSaveData
    {
        int SaveVersion { get; set; }
        void Migrate(int fromVersion);
    }

    public  class SaveVersionInfo
    {
        public static int Current = 1;
    }
    #endregion
    #region  Serialice
    public class SerialiceGameData 
    {
        static bool _SaveBusy = false;
        static bool _LoadingBusy = false;

        static SerialiceGameData()
        {
            EventLibrary.EventLibrary.GameStartLoading.AddListener(
                boolState => _LoadingBusy = boolState
            );

            EventLibrary.EventLibrary.GameendendLoading.AddListener(
                boolState => _LoadingBusy = boolState
            );
        }

        public static void Serialize_GameData<SaveData>(SaveData Game_data)
        {
            if (_SaveBusy || _LoadingBusy)
            {
                return;
            }

            _SaveBusy = true; 

            bool saved = false;
            string tempFile = null;

            try
            {
                if (string.IsNullOrEmpty(Path_List.SaveFile_Path))
                {
                    if (!Directory.Exists(Path_List.Directory_PathSaveFile))
                    {
                        Gen_File_Requirements.GenDirectory();
                    }

                    FileGen_Methods.Create_files(Path_List.GustomName);
                    Path_List.SaveFile_Path = Path.Combine(Path_List.Directory_PathSaveFile,Path_List.GustomName); 
                }

                if (!Directory.Exists(Path_List.Directory_PathSaveFile))
                {
                    return;
                }

                if (Game_data is IVersionedSaveData versionedData)
                {
                    versionedData.SaveVersion = SaveVersionInfo.Current;
                }

                string json = JsonUtility.ToJson(Game_data, true); 

                tempFile = Path_List.SaveFile_Path + ".tmp";
                string backupFile = Path_List.SaveFile_Path + ".bak";
                string previousBackupFile = backupFile + ".old";

                // Fuerza a escribir en disco, esto proboca que el Json  ocupe un Bak, un Temp y un json por lo que se hace en Flush
                using (FileStream stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush(true);
                }

                FileInfo currentFile = new FileInfo(Path_List.SaveFile_Path);


                if (!currentFile.Exists || currentFile.Length == 0)
                {
                    if (currentFile.Exists)
                    {
                        File.Delete(Path_List.SaveFile_Path);
                    }

                    File.Move(tempFile, Path_List.SaveFile_Path);
                }
                else
                {
                    if (File.Exists(backupFile))
                    {
                        if (File.Exists(previousBackupFile))
                        {
                            File.Delete(previousBackupFile);
                        }

                        File.Move(backupFile, previousBackupFile);
                    }

                    try
                    {
                        File.Replace(
                            tempFile,
                            Path_List.SaveFile_Path,
                            backupFile
                        );
                    }
                    catch
                    {
                        if (!File.Exists(backupFile) && File.Exists(previousBackupFile))
                        {
                            File.Move(previousBackupFile, backupFile);
                        }

                        // File.Replace no es confiable en todas las plataformas, se hace la copia manual.
                        File.Copy(Path_List.SaveFile_Path, backupFile, true);
                        File.Copy(tempFile, Path_List.SaveFile_Path, true);
                    }

                    if (File.Exists(previousBackupFile))
                    {
                        File.Delete(previousBackupFile);
                    }
                }

                saved = true;

                RMC_ThePlayerGeneralManager.DefaulPath = Path_List.SaveFile_Path;
            }
            catch (Exception FailOnSave)
            {
                Debug.Log($"Los Datos no se guardaron: {FailOnSave}");
            }
            finally
            {
                try
                {
                    if (tempFile != null && File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch
                {
                    // El .tmp sobrante se sobrescribe en el siguiente guardado.
                }

                _SaveBusy = false; 
            }

            if (saved)
            {
                EventLibrary.EventLibrary.Execute_GameSaveLoading(); 
            }
        }

        public static void Deserialice_GameDAta<SaveData> (string Path, Action<SaveData> onsaveFound = null, Action onSaveNotFound = null)
        {
            string BackupPath = Path + ".bak";
            string PreviousBackupPath = BackupPath + ".old";

            EventLibrary.EventLibrary.Execute_GameendstartLoading();

            SaveData Game_data = default; 
            bool Loaded = false; 

            try
            {
                string[] PathsToTry =
                {
                    Path,
                    BackupPath,
                    PreviousBackupPath
                };

                foreach (string CurrentPath in PathsToTry)
                {
                    if (!File.Exists(CurrentPath))
                    {
                        continue;
                    }

                    try
                    {
                        string json = File.ReadAllText(CurrentPath);

                        if (string.IsNullOrWhiteSpace(json))
                        {
                            continue;
                        }

                        string TrimmedJson = json.Trim();

                        if (!TrimmedJson.StartsWith("{") ||
                            !TrimmedJson.EndsWith("}"))
                        {
                            continue;
                        }

                        Game_data = JsonUtility.FromJson<SaveData>(json); 

                        if (Game_data != null)
                        {
                            if (Game_data is IVersionedSaveData versionedData &&
                                versionedData.SaveVersion < SaveVersionInfo.Current)
                            {
                                versionedData.Migrate(versionedData.SaveVersion);
                                versionedData.SaveVersion = SaveVersionInfo.Current;
                            }

                            Loaded = true;
                            Path_List.SaveFile_Path = Path;

                            if (CurrentPath != Path)
                            {
                                Debug.Log($"Guardado recuperado desde el respaldo: {CurrentPath}");
                            }

                            break;
                        }
                    }
                    catch
                    {
                        // Intentar el siguiente archivo de respaldo.
                    }
                }

                if (Loaded)
                {
                    onsaveFound?.Invoke(Game_data); 
                }
                else
                {
                    EventLibrary.EventLibrary.Execute_GameLoadFailed(Path); 
                    onSaveNotFound?.Invoke(); 
                }
            }
            finally
            {
                EventLibrary.EventLibrary.Execute_GameendendLoading();
            }
        }

        public static void Erase_serialiceData()
        {
            if (_SaveBusy || _LoadingBusy)
            {
                return;
            }

            if (string.IsNullOrEmpty(Path_List.SaveFile_Path))
            {
                return;
            }

            try
            {
                File.Delete(Path_List.SaveFile_Path); 
                File.Delete(Path_List.SaveFile_Path + ".bak"); 
                File.Delete(Path_List.SaveFile_Path + ".bak.old");
            }
            catch (Exception FailOnErase)
            {
                Debug.Log($"[F/] {FailOnErase}");
            }
        }

        public static void Duplicate_SerialiceData(string path)
        {
            if (_SaveBusy || _LoadingBusy)
            {
                return;
            }

            _SaveBusy = true; 

            try
            {
                string SourcePath = path; 
                string DirectoryOfPath = Path.GetDirectoryName(path); 
                string NameOnly = Path.GetFileNameWithoutExtension(path); 

                Match Extraer_integers = Regex.Match(NameOnly,@"\d+");
                string PAthWhitoutJson = Path.Combine(DirectoryOfPath, NameOnly); //Es el Path pero sin el .json

                int numbers_Extract = 1; 
                string pathWhitoutnumber = PAthWhitoutJson; 

                if (Extraer_integers.Success)
                {
                    if (!int.TryParse(Extraer_integers.Value, out numbers_Extract))
                    {
                        numbers_Extract = 1;
                    }

                    pathWhitoutnumber =Path.Combine(DirectoryOfPath, NameOnly.Remove(Extraer_integers.Index, Extraer_integers.Length));
                }

                string Compare_Path_ForClonation = pathWhitoutnumber + numbers_Extract.ToString() + ".json";
                while (File.Exists(Compare_Path_ForClonation) )
                {
                    ++numbers_Extract;
                    Compare_Path_ForClonation = pathWhitoutnumber + numbers_Extract.ToString() + ".json";
                    
                }

                path = Compare_Path_ForClonation;

                if (File.Exists(SourcePath))
                {
                    File.Copy(SourcePath, path);
                }
                else
                {
                    Gen_File_Requirements.GenFile(path);
                }
            }
            catch (Exception FailOnDuplicate)
            {
                Debug.Log($"[B/] {FailOnDuplicate}");
            }
            finally
            {
                _SaveBusy = false; 
            }

        }

        public static List<string> Find_AllSaveFiles()
        {
            List<string> SaveFilesFound = new List<string>(); 

            if (!Directory.Exists(Path_List.Directory_PathSaveFile))
            {
                return SaveFilesFound;
            }

            try
            {
                string[] FilesInDirectory = Directory.GetFiles(Path_List.Directory_PathSaveFile, "*.json");

                // Se lee la fecha una sola vez por archivo, en lugar de en cada comparacion del sort.
                DateTime[] WriteTimes = new DateTime[FilesInDirectory.Length];
                for (int i = 0; i < FilesInDirectory.Length; i++)
                {
                    WriteTimes[i] = File.GetLastWriteTime(FilesInDirectory[i]);
                }

                Array.Sort(WriteTimes, FilesInDirectory);
                Array.Reverse(FilesInDirectory);

                foreach (string FileFound in FilesInDirectory)
                {
                    if (FileFound.EndsWith(".json"))
                    {
                        SaveFilesFound.Add(FileFound);
                    }
                }
            }
            catch (Exception FailOnFindFiles)
            {
                Debug.Log($"[B/] {FailOnFindFiles}");
            }

            return SaveFilesFound; 
        }

        public static bool Select_SaveFile(string path)
        {
            if (!File.Exists(path))
            {
                return false; 
            }

            Path_List.SaveFile_Path = path; 
            return true; 
        }
    }
    #endregion
}