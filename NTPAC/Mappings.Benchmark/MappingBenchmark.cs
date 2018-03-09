using System;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Nelibur.ObjectMapper;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Entities.Mappers;

namespace Mappings.Benchmark
{
  public class MappingBenchmark
  {
    private readonly L7Pdu _l7Pdu;

    public MappingBenchmark()
    {
      Mapper.Initialize(cfg => cfg.CreateMap<L7Pdu, L7PduEntity>());
      TinyMapper.Bind<L7Pdu, L7PduEntity>();

      var pduFrame = new Frame(0, new Byte[0]);
      pduFrame.SetupL7PayloadDataSegment(0, 0);
      this._l7Pdu = new L7Pdu(pduFrame, FlowDirection.Up);
    }

    [Benchmark]
    public void AutoMapperBenchmark()
    {
      Mapper.Map<L7PduEntity>(this._l7Pdu);
    }

    [Benchmark]
    public void ManualMapperBenchmark()
    {
      L7PduMapper.Map(this._l7Pdu);
    }

    [Benchmark]
    public void TinyMapperBenchmark()
    {
      TinyMapper.Map<L7PduEntity>(this._l7Pdu);
    }
  }
}