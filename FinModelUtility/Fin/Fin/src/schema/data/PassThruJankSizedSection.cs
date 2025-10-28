﻿using System.Threading.Tasks;

using fin.util.tasks;

using schema.binary;

namespace fin.schema.data;

public sealed class PassThruJankSizedSection<T>(T data) : ISizedSection<T>
    where T : IBinaryConvertible {
  public uint Size { get; private set; }
  public bool UseSize { get; set; } = false;

  public T Data { get; set; } = data;

  public int TweakReadSize { get; set; }

  public void Read(IBinaryReader br) {
    this.Size = br.ReadUInt32();

    if (this.UseSize) {
      var tweakedSize = this.Size + this.TweakReadSize;
      var basePosition = br.Position;
      br.SubreadAt(br.Position, (int) tweakedSize, () => this.Data.Read(br));

      br.Position = basePosition + tweakedSize;
    } else {
      br.SubreadAt(br.Position, () => this.Data.Read(br));
    }
  }

  public void Write(IBinaryWriter bw) {
    var sizeSource = new TaskCompletionSource<uint>();
    bw.WriteUInt32Delayed(sizeSource.Task);

    var startingPositionTask = bw.GetAbsolutePosition();
    this.Data.Write(bw);
    var endPositionTask = bw.GetAbsolutePosition();

    var sizeTask = endPositionTask.Subtract(startingPositionTask);
    var tweakedSizeTask = sizeTask.Subtract(this.TweakReadSize)
                                  .CastChecked<long, uint>();

    tweakedSizeTask.ThenSetResult(sizeSource);
  }
}