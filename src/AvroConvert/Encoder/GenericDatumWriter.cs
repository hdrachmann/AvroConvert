﻿namespace EhwarSoft.Avro.Encoder
{
    using Generic;
    using Schema;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class GenericDatumWriter : AbstractEncoder
    {
        private readonly Schema _schema;

        public GenericDatumWriter(Schema schema) : base(schema, new GenericArrayAccess(), new DictionaryMapAccess())
        {
            _schema = schema;
        }

        protected override void WriteRecordFields(object recordObj, RecordFieldWriter[] writers, IWriter encoder)
        {
            GenericRecord record = new GenericRecord((RecordSchema)_schema);

            if (recordObj is Dictionary<string, object> obj)
            {
                record.contents = obj;
            }

            else
            {
                record.contents = SplitKeyValues(recordObj);
            }

            foreach (var writer in writers)
            {
                writer.WriteField(record[writer.Field.Name], encoder);
            }
        }


        protected override void EnsureRecordObject(RecordSchema recordSchema, object value)
        {
            if (value == null || !(value is GenericRecord) || !((value as GenericRecord).Schema.Equals(recordSchema)))
            {
                throw TypeMismatch(value, "record", "GenericRecord");
            }
        }

        protected override void WriteField(object record, string fieldName, int fieldPos, WriteItem writer,
            IWriter encoder)
        {
            writer(((GenericRecord)record)[fieldName], encoder);
        }

        protected override WriteItem ResolveEnum(EnumSchema es)
        {
            return (v, e) =>
            {
                if (v == null || !(v is GenericEnum) || !((v as GenericEnum).Schema.Equals(es)))
                    throw TypeMismatch(v, "enum", "GenericEnum");
                e.WriteEnum(es.Ordinal((v as GenericEnum).Value));
            };
        }

        protected override void WriteFixed(FixedSchema es, object value, IWriter encoder)
        {
            if (value == null || !(value is GenericFixed) || !(value as GenericFixed).Schema.Equals(es))
            {
                throw TypeMismatch(value, "fixed", "GenericFixed");
            }

            GenericFixed ba = (GenericFixed)value;
            encoder.WriteFixed(ba.Value);
        }


        /*
         * FIXME: This method of determining the Union branch has problems. If the data is IDictionary<string, object>
         * if there are two branches one with record schema and the other with map, it choose the first one. Similarly if
         * the data is byte[] and there are fixed and bytes schemas as branches, it choose the first one that matches.
         * Also it does not recognize the arrays of primitive types.
         */
        protected override bool UnionBranchMatches(Schema sc, object obj)
        {
            if (obj == null && sc.Tag != Schema.Type.Null) return false;
            switch (sc.Tag)
            {
                case Schema.Type.Null:
                    return obj == null;
                case Schema.Type.Boolean:
                    return obj is bool;
                case Schema.Type.Int:
                    return obj is int;
                case Schema.Type.Long:
                    return obj is long;
                case Schema.Type.Float:
                    return obj is float;
                case Schema.Type.Double:
                    return obj is double;
                case Schema.Type.Bytes:
                    return obj is byte[];
                case Schema.Type.String:
                    return obj is string;
                case Schema.Type.Error:
                case Schema.Type.Record:
                    //return obj is GenericRecord && (obj as GenericRecord).Schema.Equals(s);
                    return obj is GenericRecord &&
                           (obj as GenericRecord).Schema.SchemaName.Equals((sc as RecordSchema).SchemaName);
                case Schema.Type.Enumeration:
                    //return obj is GenericEnum && (obj as GenericEnum).Schema.Equals(s);
                    return obj is GenericEnum &&
                           (obj as GenericEnum).Schema.SchemaName.Equals((sc as EnumSchema).SchemaName);
                case Schema.Type.Array:
                    return obj is Array && !(obj is byte[]);
                case Schema.Type.Map:
                    return obj is IDictionary<string, object>;
                case Schema.Type.Union:
                    return false; // Union directly within another union not allowed!
                case Schema.Type.Fixed:
                    //return obj is GenericFixed && (obj as GenericFixed).Schema.Equals(s);
                    return obj is GenericFixed &&
                           (obj as GenericFixed).Schema.SchemaName.Equals((sc as FixedSchema).SchemaName);
                default:
                    throw new AvroException("Unknown schema type: " + sc.Tag);
            }
        }

        private class GenericArrayAccess : ArrayAccess
        {
            public object EnsureArrayObject(object value)
            {
                int length = 0;

                if (value == null)
                {
                    return null;
                }

                if (value is OrderedDictionary dictionary)
                {
                    length = dictionary.Count;

                    object[] result = new object[length];

                    for (int i = 0; i < length; i++)
                    {
//                        KeyValue keyValue= new KeyValue();
//                        keyValue.Key = dictionary.Keys.;
//
//                        result[i] = dictionary.Values
                    }

                    return result;
                }
                else
                {
                    IList list = value as IList;
                    length = list.Count;

                    object[] result = new object[length];

                    for (int i = 0; i < length; i++)
                    {
                        result[i] = list[i];
                    }

                    return result;
                }


                //if (value == null || !(value is Array)) throw TypeMismatch(value, "array", "Array");
            }

            public long GetArrayLength(object value)
            {
                return ((Array)value)?.Length ?? 0;
            }

            public void WriteArrayValues(object array, WriteItem valueWriter, IWriter encoder)
            {
                if (array == null)
                {
                    valueWriter(null, encoder);
                }
                else
                {
                    var arrayInstance = (Array)array;
                    for (int i = 0; i < arrayInstance.Length; i++)
                    {
                        encoder.StartItem();
                        valueWriter(arrayInstance.GetValue(i), encoder);
                    }
                }
            }
        }
    }
}