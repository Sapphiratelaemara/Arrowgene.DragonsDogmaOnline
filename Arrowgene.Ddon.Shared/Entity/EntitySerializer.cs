﻿using System;
using System.Collections.Generic;
using System.Text;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;

namespace Arrowgene.Ddon.Shared.Entity
{
    public abstract class EntitySerializer
    {
        private static readonly Dictionary<Type, EntitySerializer> Serializers =
            new Dictionary<Type, EntitySerializer>(
                new[]
                {
                    Create(new CDataCharacterListElementSerializer()),
                    Create(new CDataCharacterListInfoSerializer()),
                    Create(new CDataEditInfoSerializer()),
                    Create(new CDataEquipElementParamSerializer()),
                    Create(new CDataEquipElementUnkTypeSerializer()),
                    Create(new CDataEquipElementUnkType2Serializer()),
                    Create(new CDataEquipItemInfoSerializer()),
                    Create(new CDataGPCourseValidSerializer()),
                    Create(new CDataMatchingProfileSerializer()),
                    Create(new DoubleByteThingSerializer()),
                }
            );

        private static KeyValuePair<Type, EntitySerializer> Create(EntitySerializer serializer)
        {
            return new KeyValuePair<Type, EntitySerializer>(serializer.GetEntityType(), serializer);
        }

        public static void RegisterReader(EntitySerializer reader)
        {
            Serializers.Add(reader.GetEntityType(), reader);
        }

        public static EntitySerializer<T> Get<T>()
        {
            Type type = typeof(T);
            object obj = Serializers[type];
            EntitySerializer<T> serializer = obj as EntitySerializer<T>;
            return serializer;
        }

        protected abstract Type GetEntityType();
    }

    public abstract class EntitySerializer<T> : EntitySerializer
    {
        public abstract void Write(IBuffer buffer, T obj);
        public abstract T Read(IBuffer buffer);

        public List<T> ReadList(IBuffer buffer)
        {
            return ReadEntityList<T>(buffer);
        }

        public void WriteList(IBuffer buffer, List<T> entities)
        {
            WriteEntityList<T>(buffer, entities);
        }

        protected override Type GetEntityType()
        {
            return typeof(T);
        }

        protected void WriteUInt64(IBuffer buffer, ulong value)
        {
            buffer.WriteUInt64(value, Endianness.Big);
        }

        protected ulong ReadUInt64(IBuffer buffer)
        {
            return buffer.ReadUInt64(Endianness.Big);
        }

        protected void WriteUInt32(IBuffer buffer, uint value)
        {
            buffer.WriteUInt32(value, Endianness.Big);
        }

        protected uint ReadUInt32(IBuffer buffer)
        {
            return buffer.ReadUInt32(Endianness.Big);
        }

        protected void WriteUInt16(IBuffer buffer, ushort value)
        {
            buffer.WriteUInt16(value, Endianness.Big);
        }

        protected ushort ReadUInt16(IBuffer buffer)
        {
            return buffer.ReadUInt16(Endianness.Big);
        }
        
        protected void WriteBool(IBuffer buffer, bool value)
        {
            buffer.WriteBool(value);
        }

        protected void WriteByte(IBuffer buffer, byte value)
        {
            buffer.WriteByte(value);
        }
        
        protected bool ReadBool(IBuffer buffer)
        {
            return buffer.ReadBool();
        }
        
        protected byte ReadByte(IBuffer buffer)
        {
            return buffer.ReadByte();
        }

        protected void WriteMtString(IBuffer buffer, string str)
        {
            byte[] utf8 = Encoding.UTF8.GetBytes(str);
            buffer.WriteUInt16((ushort)utf8.Length, Endianness.Big);
            buffer.WriteBytes(utf8);
        }

        protected string ReadMtString(IBuffer buffer)
        {
            ushort len = buffer.ReadUInt16(Endianness.Big);
            string str = buffer.ReadString(len, Encoding.UTF8);
            return str;
        }

        protected void WriteEntity<TEntity>(IBuffer buffer, TEntity entity)
        {
            EntitySerializer<TEntity> serializer = Get<TEntity>();
            if (serializer == null)
            {
                // error
                return;
            }

            serializer.Write(buffer, entity);
        }

        protected void WriteEntityList<TEntity>(IBuffer buffer, List<TEntity> entities)
        {
            WriteUInt32(buffer, (uint) entities.Count);
            for (int i = 0; i < entities.Count; i++)
            {
                WriteEntity(buffer, entities[i]);
            }
        }

        protected List<TEntity> ReadEntityList<TEntity>(IBuffer buffer)
        {
            List<TEntity> entities = new List<TEntity>();
            uint len = ReadUInt32(buffer);
            for (int i = 0; i < len; i++)
            {
                entities.Add(ReadEntity<TEntity>(buffer));
            }

            return entities;
        }

        protected TEntity ReadEntity<TEntity>(IBuffer buffer)
        {
            EntitySerializer<TEntity> serializer = Get<TEntity>();
            if (serializer == null)
            {
                // error
                return default;
            }

            return serializer.Read(buffer);
        }

        public byte[] Write(T entity)
        {
            IBuffer buffer = new StreamBuffer();
            Write(buffer, entity);
            return buffer.GetAllBytes();
        }

        public T Read(byte[] data)
        {
            IBuffer buffer = new StreamBuffer(data);
            buffer.SetPositionStart();
            return Read(buffer);
        }
    }
}