﻿using System.Numerics;

using fin.math.rotations;

namespace fin.ui;

public sealed class Camera : ICamera {
  // TODO: Add x/y/z locking.

  public static Camera NewLookingAt(float x,
                                    float y,
                                    float z,
                                    float yaw,
                                    float pitch,
                                    float distance) {
    var camera = new Camera { YawDegrees = yaw, PitchDegrees = pitch };
    camera.Position = new Vector3(x, y, z) - camera.Normal * distance;
    return camera;
  }

  public static ICamera Instance { get; private set; }

  public Camera() {
    Instance = this;
  }

  public Vector3 Position { get; set; }


  /// <summary>
  ///   The left-right angle of the camera, in degrees.
  /// </summary>
  public float YawDegrees { get; set; }

  /// <summary>
  ///   The up-down angle of the camera, in degrees.
  /// </summary>
  public float PitchDegrees { get; set; }


  public float HorizontalNormal
    => FinTrig.Cos(this.PitchDegrees * FinTrig.DEG_2_RAD);

  public float VerticalNormal
    => FinTrig.Sin(this.PitchDegrees * FinTrig.DEG_2_RAD);


  public Vector3 Normal {
    get {
      var horizontalNormal = this.HorizontalNormal;
      var verticalNormal = this.VerticalNormal;
      return new Vector3(
          horizontalNormal *
          FinTrig.Cos(this.YawDegrees * FinTrig.DEG_2_RAD),
          horizontalNormal *
          FinTrig.Sin(this.YawDegrees * FinTrig.DEG_2_RAD),
          verticalNormal);
    }
  }

  public Vector3 Up {
    get {
      var horizontalNormal = this.HorizontalNormal;
      var verticalNormal = this.VerticalNormal;
      return new Vector3(
          -verticalNormal * FinTrig.Cos(this.YawDegrees * FinTrig.DEG_2_RAD),
          -verticalNormal * FinTrig.Sin(this.YawDegrees * FinTrig.DEG_2_RAD),
          horizontalNormal);
    }
  }

  // TODO: These negative signs and flipped cos/sin don't look right but they
  // work???
  public void Move(float forwardVector,
                   float rightVector,
                   float upVector,
                   float speed) {
    var deltaZ = speed *
              (this.VerticalNormal * forwardVector +
               this.HorizontalNormal * upVector);

    var forwardYawRads = this.YawDegrees * FinTrig.DEG_2_RAD;
    var rightYawRads = (this.YawDegrees - 90) * FinTrig.DEG_2_RAD;

    var deltaX =
        speed *
        (this.HorizontalNormal *
         (forwardVector * FinTrig.Cos(forwardYawRads) +
          rightVector * FinTrig.Cos(rightYawRads)) +
         -this.VerticalNormal * upVector * FinTrig.Cos(forwardYawRads));

    var deltaY =
        speed *
        (this.HorizontalNormal *
         (forwardVector * FinTrig.Sin(forwardYawRads) +
          rightVector * FinTrig.Sin(rightYawRads)) +
         -this.VerticalNormal * upVector * FinTrig.Sin(forwardYawRads));

    this.Position += new Vector3(deltaX, deltaY, deltaZ);
  }
}