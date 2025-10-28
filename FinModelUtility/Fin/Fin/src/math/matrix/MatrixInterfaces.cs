﻿using System;

using readOnly;

namespace fin.math.matrix;
// The type parameters on these matrices are kind of janky, but they allow us
// to have consistent interfaces between 3x3 and 4x4 matrices.

[GenerateReadOnly]
public partial interface IFinMatrix<[KeepMutableType] TMutable, TReadOnly, TImpl>
    : IEquatable<TReadOnly>
    where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
    where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
  new TImpl Impl { get; set; }

  void CopyFrom(TReadOnly other);
  void CopyFrom(in TImpl other);

  TMutable SetIdentity();
  TMutable SetZero();

  new float this[int row, int column] { get; set; }

  TMutable AddInPlace(TReadOnly other);
  TMutable AddInPlace(in TImpl other);
  TMutable MultiplyInPlace(TReadOnly other);
  TMutable MultiplyInPlace(in TImpl other);
  TMutable MultiplyInPlace(float other);

  TMutable InvertInPlace();

  [Const]
  new TMutable Clone();

  [Const]
  new TMutable CloneAndAdd(TReadOnly other);
  [Const]
  new void AddIntoBuffer(TReadOnly other, TMutable buffer);

  [Const]
  new TMutable CloneAndMultiply(TReadOnly other);
  [Const]
  new void MultiplyIntoBuffer(TReadOnly other, TMutable buffer);

  [Const]
  new TMutable CloneAndAdd(in TImpl other);
  [Const]
  new void AddIntoBuffer(in TImpl other, TMutable buffer);

  [Const]
  new TMutable CloneAndMultiply(in TImpl other);
  [Const]
  new void MultiplyIntoBuffer(in TImpl other, TMutable buffer);

  [Const]
  new TMutable CloneAndMultiply(float other);
  [Const]
  new void MultiplyIntoBuffer(float other, TMutable buffer);

  [Const]
  new TMutable CloneAndInvert();
  [Const]
  new void InvertIntoBuffer(TMutable buffer);
}