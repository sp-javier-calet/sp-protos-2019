using System.Collections;
using System.IO;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Standalone
{
    public class TaskSerializationDataSerializer
    {
        public static readonly TaskSerializationDataSerializer Instance = new TaskSerializationDataSerializer();

        public void Serialize(BehaviorSource behaviorSourceTemp, TaskSerializationData taskData, IWriter writer)
        {
            int size = 0;

            // types
            size = (taskData.types != null) ? taskData.types.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.types[i]);
            }

            // parentIndex
            size = (taskData.parentIndex != null) ? taskData.parentIndex.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.parentIndex[i]);
            }

            // startIndex
            size = (taskData.startIndex != null) ? taskData.startIndex.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.startIndex[i]);
            }

            // variableStartIndex
            size = (taskData.variableStartIndex != null) ? taskData.variableStartIndex.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.variableStartIndex[i]);
            }

            // JSONSerialization
            writer.Write(taskData.JSONSerialization);

            // taskData.fieldSerializationData.typeName
            size = (taskData.fieldSerializationData.typeName != null) ? taskData.fieldSerializationData.typeName.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.fieldSerializationData.typeName[i]);
            }

            // taskData.fieldSerializationData.fieldNameHash
            size = (taskData.fieldSerializationData.fieldNameHash != null) ? taskData.fieldSerializationData.fieldNameHash.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.fieldSerializationData.fieldNameHash[i]);
            }

            // taskData.fieldSerializationData.startIndex
            size = (taskData.fieldSerializationData.startIndex != null) ? taskData.fieldSerializationData.startIndex.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.fieldSerializationData.startIndex[i]);
            }

            // taskData.fieldSerializationData.dataPosition
            size = (taskData.fieldSerializationData.dataPosition != null) ? taskData.fieldSerializationData.dataPosition.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.fieldSerializationData.dataPosition[i]);
            }

            // taskData.fieldSerializationData.byteData
            size = (taskData.fieldSerializationData.byteData != null) ? taskData.fieldSerializationData.byteData.Count : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.fieldSerializationData.byteData[i]);
            }

            // taskData.fieldSerializationData.byteDataArray
            size = (taskData.fieldSerializationData.byteDataArray != null) ? taskData.fieldSerializationData.byteDataArray.Length : 0;
            writer.Write(size);
            for(int i = 0; i < size; ++i)
            {
                writer.Write(taskData.fieldSerializationData.byteDataArray[i]);
            }

            // Version
            writer.Write(taskData.Version);

            // TaskId FieldType FieldName to index
//            BinaryDeserialization.Load(taskData, behaviorSourceTemp);
            System.Collections.Generic.List<string> indexKeys = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<int> indexValues = new System.Collections.Generic.List<int>();

            var enumerator = BinaryDeserialization.IndexMapRead.GetEnumerator();
            while(enumerator.MoveNext())
            {
                indexKeys.Add( enumerator.Current.Key );
                indexValues.Add( enumerator.Current.Value );
            }

            writer.Write(indexKeys.Count);
            for(int i = 0; i < indexKeys.Count; ++i)
            {
                writer.Write(indexKeys[i]);
                writer.Write(indexValues[i]);
            }
        }

        public void Deserialize(TaskSerializationData taskData, IReader reader)
        {
            int size = 0;

            // types
            size = reader.ReadInt32();
            taskData.types = new System.Collections.Generic.List<string>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.types.Add(reader.ReadString());
            }

            // parentIndex
            size = reader.ReadInt32();
            taskData.parentIndex = new System.Collections.Generic.List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.parentIndex.Add(reader.ReadInt32());
            }

            // startIndex
            size = reader.ReadInt32();
            taskData.startIndex = new System.Collections.Generic.List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.startIndex.Add(reader.ReadInt32());
            }

            // variableStartIndex
            size = reader.ReadInt32();
            taskData.variableStartIndex = new System.Collections.Generic.List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.variableStartIndex.Add(reader.ReadInt32());
            }

            // JSONSerialization
            taskData.JSONSerialization = reader.ReadString();

            // taskData.fieldSerializationData.typeName
            size = reader.ReadInt32();
            taskData.fieldSerializationData.typeName = new System.Collections.Generic.List<string>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.fieldSerializationData.typeName.Add(reader.ReadString());
            }

            // taskData.fieldSerializationData.fieldNameHash
            size = reader.ReadInt32();
            taskData.fieldSerializationData.fieldNameHash = new System.Collections.Generic.List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.fieldSerializationData.fieldNameHash.Add(reader.ReadInt32());
            }

            // taskData.fieldSerializationData.startIndex
            size = reader.ReadInt32();
            taskData.fieldSerializationData.startIndex = new System.Collections.Generic.List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.fieldSerializationData.startIndex.Add(reader.ReadInt32());
            }

            // taskData.fieldSerializationData.dataPosition
            size = reader.ReadInt32();
            taskData.fieldSerializationData.dataPosition = new System.Collections.Generic.List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.fieldSerializationData.dataPosition.Add(reader.ReadInt32());
            }

            // taskData.fieldSerializationData.byteData
            size = reader.ReadInt32();
            taskData.fieldSerializationData.byteData = new System.Collections.Generic.List<byte>(size);
            for(int i = 0; i < size; ++i)
            {
                taskData.fieldSerializationData.byteData.Add(reader.ReadByte());
            }

            // taskData.fieldSerializationData.byteDataArray
            size = reader.ReadInt32();
            taskData.fieldSerializationData.byteDataArray = new byte[size];
            for(int i = 0; i < size; ++i)
            {
                taskData.fieldSerializationData.byteDataArray[i] = reader.ReadByte();
            }

            // Version
            taskData.Version = reader.ReadString();

            // IndexMap
            size = reader.ReadInt32();
            taskData.fieldSerializationData.IndexKeys = new List<string>(size);
            taskData.fieldSerializationData.IndexValues = new List<int>(size);
            for(int i = 0; i < size; ++i)
            {
                string key = reader.ReadString();
                int value = reader.ReadInt32();

                taskData.fieldSerializationData.IndexKeys.Add(key);
                taskData.fieldSerializationData.IndexValues.Add(value);
            }
        }
    }

    public class BehaviorSourceSerializer
    {
        public static readonly BehaviorSourceSerializer Instance = new BehaviorSourceSerializer();
        
        public void Serialize(BehaviorSource behaviorSource, IWriter writer)
        {
            writer.Write(behaviorSource.behaviorName);
            writer.Write(behaviorSource.behaviorDescription);

            TaskSerializationDataSerializer.Instance.Serialize(behaviorSource, behaviorSource.TaskData, writer);
        }

        public void Deserialize(BehaviorSource behaviorSource, IReader reader)
        {
            behaviorSource.behaviorName = reader.ReadString();
            behaviorSource.behaviorDescription = reader.ReadString();

            behaviorSource.TaskData = new TaskSerializationData();
            TaskSerializationDataSerializer.Instance.Deserialize(behaviorSource.TaskData, reader);
        }

        public void SaveFile(BehaviorSource behaviorSource, string filePath)
        {
            try
            {
                var stream = new FileStream(filePath, FileMode.OpenOrCreate);
                BehaviorSourceSerializer.Instance.Serialize(behaviorSource, new SystemBinaryWriter(stream));
                stream.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error trying to save BinaryBehaviorSource: " + e.Message);
            }
        }

        public void LoadFile(BehaviorSource behaviorSource, string filePath)
        {
            try
            {
                var stream = new FileStream(filePath, FileMode.Open);
                BehaviorSourceSerializer.Instance.Deserialize(behaviorSource, new SystemBinaryReader(stream));
                stream.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error trying to load BinaryBehaviorSource: " + e.Message);
            }
        }
    }
}
