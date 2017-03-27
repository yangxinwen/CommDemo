﻿// Generated by the protocol buffer compiler.  DO NOT EDIT!

using pb = global::Google.ProtocolBuffers;
using pbc = global::Google.ProtocolBuffers.Collections;
using pbd = global::Google.ProtocolBuffers.Descriptors;
using scg = global::System.Collections.Generic;
namespace DuiCommun
{

    public static partial class Cfs
    {

        #region Extension registration
        public static void RegisterAllExtensions(pb::ExtensionRegistry registry)
        {
        }
        #endregion
        #region Static variables
        internal static pbd::MessageDescriptor internal__static_DuiCommun_Request__Descriptor;
        internal static pb::FieldAccess.FieldAccessorTable<global::DuiCommun.Request, global::DuiCommun.Request.Builder> internal__static_DuiCommun_Request__FieldAccessorTable;
        #endregion
        #region Descriptor
        public static pbd::FileDescriptor Descriptor
        {
            get { return descriptor; }
        }
        private static pbd::FileDescriptor descriptor;

        static Cfs()
        {
            byte[] descriptorData = global::System.Convert.FromBase64String(
                "CgljZnMucHJvdG8SCUR1aUNvbW11biJmCgdSZXF1ZXN0EhIKClNlcXVlbmNl" +
                "SWQYASACKAcSEQoJU2Vzc2lvbklkGAIgAigHEg8KB0Z1bkNvZGUYAyACKAcS" +
                "EAoISnNvbkJvZHkYBCABKAkSEQoJRGF0YUJ5dGVzGAUgASgM");
            pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate (pbd::FileDescriptor root) {
                descriptor = root;
                internal__static_DuiCommun_Request__Descriptor = Descriptor.MessageTypes[0];
                internal__static_DuiCommun_Request__FieldAccessorTable =
                    new pb::FieldAccess.FieldAccessorTable<global::DuiCommun.Request, global::DuiCommun.Request.Builder>(internal__static_DuiCommun_Request__Descriptor,
                        new string[] { "SequenceId", "SessionId", "FunCode", "JsonBody", "DataBytes", });
                return null;
            };
            pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
                new pbd::FileDescriptor[] {
                }, assigner);
        }
        #endregion

    }
    #region Messages
    public sealed partial class Request : pb::GeneratedMessage<Request, Request.Builder>
    {
        private static readonly Request defaultInstance = new Builder().BuildPartial();
        public static Request DefaultInstance
        {
            get { return defaultInstance; }
        }

        public override Request DefaultInstanceForType
        {
            get { return defaultInstance; }
        }

        protected override Request ThisMessage
        {
            get { return this; }
        }

        public static pbd::MessageDescriptor Descriptor
        {
            get { return global::DuiCommun.Cfs.internal__static_DuiCommun_Request__Descriptor; }
        }

        protected override pb::FieldAccess.FieldAccessorTable<Request, Request.Builder> InternalFieldAccessors
        {
            get { return global::DuiCommun.Cfs.internal__static_DuiCommun_Request__FieldAccessorTable; }
        }

        public const int SequenceIdFieldNumber = 1;
        private bool hasSequenceId;
        private uint sequenceId_ = 0;
        public bool HasSequenceId
        {
            get { return hasSequenceId; }
        }
        [global::System.CLSCompliant(false)]
        public uint SequenceId
        {
            get { return sequenceId_; }
        }

        public const int SessionIdFieldNumber = 2;
        private bool hasSessionId;
        private uint sessionId_ = 0;
        public bool HasSessionId
        {
            get { return hasSessionId; }
        }
        [global::System.CLSCompliant(false)]
        public uint SessionId
        {
            get { return sessionId_; }
        }

        public const int FunCodeFieldNumber = 3;
        private bool hasFunCode;
        private uint funCode_ = 0;
        public bool HasFunCode
        {
            get { return hasFunCode; }
        }
        [global::System.CLSCompliant(false)]
        public uint FunCode
        {
            get { return funCode_; }
        }

        public const int JsonBodyFieldNumber = 4;
        private bool hasJsonBody;
        private string jsonBody_ = "";
        public bool HasJsonBody
        {
            get { return hasJsonBody; }
        }
        public string JsonBody
        {
            get { return jsonBody_; }
        }

        public const int DataBytesFieldNumber = 5;
        private bool hasDataBytes;
        private pb::ByteString dataBytes_ = pb::ByteString.Empty;
        public bool HasDataBytes
        {
            get { return hasDataBytes; }
        }
        public pb::ByteString DataBytes
        {
            get { return dataBytes_; }
        }

        public override bool IsInitialized
        {
            get
            {
                if (!hasSequenceId) return false;
                if (!hasSessionId) return false;
                if (!hasFunCode) return false;
                return true;
            }
        }

        public override void WriteTo(pb::CodedOutputStream output)
        {
            int size = SerializedSize;
            if (HasSequenceId)
            {
                output.WriteFixed32(1, SequenceId);
            }
            if (HasSessionId)
            {
                output.WriteFixed32(2, SessionId);
            }
            if (HasFunCode)
            {
                output.WriteFixed32(3, FunCode);
            }
            if (HasJsonBody)
            {
                output.WriteString(4, JsonBody);
            }
            if (HasDataBytes)
            {
                output.WriteBytes(5, DataBytes);
            }
            UnknownFields.WriteTo(output);
        }

        private int memoizedSerializedSize = -1;
        public override int SerializedSize
        {
            get
            {
                int size = memoizedSerializedSize;
                if (size != -1) return size;

                size = 0;
                if (HasSequenceId)
                {
                    size += pb::CodedOutputStream.ComputeFixed32Size(1, SequenceId);
                }
                if (HasSessionId)
                {
                    size += pb::CodedOutputStream.ComputeFixed32Size(2, SessionId);
                }
                if (HasFunCode)
                {
                    size += pb::CodedOutputStream.ComputeFixed32Size(3, FunCode);
                }
                if (HasJsonBody)
                {
                    size += pb::CodedOutputStream.ComputeStringSize(4, JsonBody);
                }
                if (HasDataBytes)
                {
                    size += pb::CodedOutputStream.ComputeBytesSize(5, DataBytes);
                }
                size += UnknownFields.SerializedSize;
                memoizedSerializedSize = size;
                return size;
            }
        }

        public static Request ParseFrom(pb::ByteString data)
        {
            return ((Builder)CreateBuilder().MergeFrom(data)).BuildParsed();
        }
        public static Request ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry)
        {
            return ((Builder)CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
        }
        public static Request ParseFrom(byte[] data)
        {
            return ((Builder)CreateBuilder().MergeFrom(data)).BuildParsed();
        }
        public static Request ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry)
        {
            return ((Builder)CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
        }
        public static Request ParseFrom(global::System.IO.Stream input)
        {
            return ((Builder)CreateBuilder().MergeFrom(input)).BuildParsed();
        }
        public static Request ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry)
        {
            return ((Builder)CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
        }
        public static Request ParseDelimitedFrom(global::System.IO.Stream input)
        {
            return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
        }
        public static Request ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry)
        {
            return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
        }
        public static Request ParseFrom(pb::CodedInputStream input)
        {
            return ((Builder)CreateBuilder().MergeFrom(input)).BuildParsed();
        }
        public static Request ParseFrom(pb::CodedInputStream input, pb::ExtensionRegistry extensionRegistry)
        {
            return ((Builder)CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
        }
        public static Builder CreateBuilder() { return new Builder(); }
        public override Builder ToBuilder() { return CreateBuilder(this); }
        public override Builder CreateBuilderForType() { return new Builder(); }
        public static Builder CreateBuilder(Request prototype)
        {
            return (Builder)new Builder().MergeFrom(prototype);
        }

        public sealed partial class Builder : pb::GeneratedBuilder<Request, Builder>
        {
            protected override Builder ThisBuilder
            {
                get { return this; }
            }
            public Builder() { }

            Request result = new Request();

            protected override Request MessageBeingBuilt
            {
                get { return result; }
            }

            public override Builder Clear()
            {
                result = new Request();
                return this;
            }

            public override Builder Clone()
            {
                return new Builder().MergeFrom(result);
            }

            public override pbd::MessageDescriptor DescriptorForType
            {
                get { return global::DuiCommun.Request.Descriptor; }
            }

            public override Request DefaultInstanceForType
            {
                get { return global::DuiCommun.Request.DefaultInstance; }
            }

            public override Request BuildPartial()
            {
                if (result == null)
                {
                    throw new global::System.InvalidOperationException("build() has already been called on this Builder");
                }
                Request returnMe = result;
                result = null;
                return returnMe;
            }

            public override Builder MergeFrom(pb::IMessage other)
            {
                if (other is Request)
                {
                    return MergeFrom((Request)other);
                }
                else
                {
                    base.MergeFrom(other);
                    return this;
                }
            }

            public override Builder MergeFrom(Request other)
            {
                if (other == global::DuiCommun.Request.DefaultInstance) return this;
                if (other.HasSequenceId)
                {
                    SequenceId = other.SequenceId;
                }
                if (other.HasSessionId)
                {
                    SessionId = other.SessionId;
                }
                if (other.HasFunCode)
                {
                    FunCode = other.FunCode;
                }
                if (other.HasJsonBody)
                {
                    JsonBody = other.JsonBody;
                }
                if (other.HasDataBytes)
                {
                    DataBytes = other.DataBytes;
                }
                this.MergeUnknownFields(other.UnknownFields);
                return this;
            }

            public override Builder MergeFrom(pb::CodedInputStream input)
            {
                return MergeFrom(input, pb::ExtensionRegistry.Empty);
            }

            public override Builder MergeFrom(pb::CodedInputStream input, pb::ExtensionRegistry extensionRegistry)
            {
                pb::UnknownFieldSet.Builder unknownFields = null;
                while (true)
                {
                    uint tag = input.ReadTag();
                    switch (tag)
                    {
                        case 0:
                            {
                                if (unknownFields != null)
                                {
                                    this.UnknownFields = unknownFields.Build();
                                }
                                return this;
                            }
                        default:
                            {
                                if (pb::WireFormat.IsEndGroupTag(tag))
                                {
                                    if (unknownFields != null)
                                    {
                                        this.UnknownFields = unknownFields.Build();
                                    }
                                    return this;
                                }
                                if (unknownFields == null)
                                {
                                    unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
                                }
                                ParseUnknownField(input, unknownFields, extensionRegistry, tag);
                                break;
                            }
                        case 13:
                            {
                                SequenceId = input.ReadFixed32();
                                break;
                            }
                        case 21:
                            {
                                SessionId = input.ReadFixed32();
                                break;
                            }
                        case 29:
                            {
                                FunCode = input.ReadFixed32();
                                break;
                            }
                        case 34:
                            {
                                JsonBody = input.ReadString();
                                break;
                            }
                        case 42:
                            {
                                DataBytes = input.ReadBytes();
                                break;
                            }
                    }
                }
            }


            public bool HasSequenceId
            {
                get { return result.HasSequenceId; }
            }
            [global::System.CLSCompliant(false)]
            public uint SequenceId
            {
                get { return result.SequenceId; }
                set { SetSequenceId(value); }
            }
            [global::System.CLSCompliant(false)]
            public Builder SetSequenceId(uint value)
            {
                result.hasSequenceId = true;
                result.sequenceId_ = value;
                return this;
            }
            public Builder ClearSequenceId()
            {
                result.hasSequenceId = false;
                result.sequenceId_ = 0;
                return this;
            }

            public bool HasSessionId
            {
                get { return result.HasSessionId; }
            }
            [global::System.CLSCompliant(false)]
            public uint SessionId
            {
                get { return result.SessionId; }
                set { SetSessionId(value); }
            }
            [global::System.CLSCompliant(false)]
            public Builder SetSessionId(uint value)
            {
                result.hasSessionId = true;
                result.sessionId_ = value;
                return this;
            }
            public Builder ClearSessionId()
            {
                result.hasSessionId = false;
                result.sessionId_ = 0;
                return this;
            }

            public bool HasFunCode
            {
                get { return result.HasFunCode; }
            }
            [global::System.CLSCompliant(false)]
            public uint FunCode
            {
                get { return result.FunCode; }
                set { SetFunCode(value); }
            }
            [global::System.CLSCompliant(false)]
            public Builder SetFunCode(uint value)
            {
                result.hasFunCode = true;
                result.funCode_ = value;
                return this;
            }
            public Builder ClearFunCode()
            {
                result.hasFunCode = false;
                result.funCode_ = 0;
                return this;
            }

            public bool HasJsonBody
            {
                get { return result.HasJsonBody; }
            }
            public string JsonBody
            {
                get { return result.JsonBody; }
                set { SetJsonBody(value); }
            }
            public Builder SetJsonBody(string value)
            {
                pb::ThrowHelper.ThrowIfNull(value, "value");
                result.hasJsonBody = true;
                result.jsonBody_ = value;
                return this;
            }
            public Builder ClearJsonBody()
            {
                result.hasJsonBody = false;
                result.jsonBody_ = "";
                return this;
            }

            public bool HasDataBytes
            {
                get { return result.HasDataBytes; }
            }
            public pb::ByteString DataBytes
            {
                get { return result.DataBytes; }
                set { SetDataBytes(value); }
            }
            public Builder SetDataBytes(pb::ByteString value)
            {
                pb::ThrowHelper.ThrowIfNull(value, "value");
                result.hasDataBytes = true;
                result.dataBytes_ = value;
                return this;
            }
            public Builder ClearDataBytes()
            {
                result.hasDataBytes = false;
                result.dataBytes_ = pb::ByteString.Empty;
                return this;
            }
        }
        static Request()
        {
            object.ReferenceEquals(global::DuiCommun.Cfs.Descriptor, null);
        }
    }

    #endregion

}
