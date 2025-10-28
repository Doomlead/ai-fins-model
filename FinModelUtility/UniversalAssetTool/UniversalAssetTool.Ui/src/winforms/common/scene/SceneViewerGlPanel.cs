﻿using System.Windows.Forms;

using fin.animation;
using fin.model;
using fin.scene;
using fin.scene.components;
using fin.ui;
using fin.ui.rendering;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;

using uni.config;
using uni.model;

namespace uni.ui.winforms.common.scene;

public sealed class SceneViewerGlPanel : BGlPanel, ISceneViewer {
  private readonly SceneViewerGl viewerImpl_ = new();

  private bool isMouseDown_ = false;
  private (int, int)? prevMousePosition_ = null;

  private bool isForwardDown_ = false;
  private bool isBackwardDown_ = false;
  private bool isLeftwardDown_ = false;
  private bool isRightwardDown_ = false;
  private bool isUpwardDown_ = false;
  private bool isDownwardDown_ = false;
  private bool isSpeedupActive_ = false;
  private bool isSlowdownActive_ = false;

  public SceneViewerGlPanel() {
      var inputTarget = this.impl_;

      inputTarget.MouseDown += (_, args) => {
                                 this.impl_.Focus();
                                 if (args.Button == MouseButtons.Left ||
                                     args.Button == MouseButtons.Right) {
                                   this.isMouseDown_ = true;
                                   this.prevMousePosition_ = null;
                                 }
                               };
      inputTarget.MouseUp += (_, args) => {
                               if (args.Button == MouseButtons.Left ||
                                   args.Button == MouseButtons.Right) {
                                 this.isMouseDown_ = false;
                               }
                             };
      inputTarget.MouseMove += (_, args) => {
                                 if (this.isMouseDown_) {
                                   var mouseLocation = (args.X, args.Y);

                                   if (this.prevMousePosition_ != null) {
                                     var (prevMouseX, prevMouseY)
                                         = this.prevMousePosition_.Value;
                                     var (mouseX, mouseY) = mouseLocation;

                                     var deltaMouseX = mouseX - prevMouseX;
                                     var deltaMouseY = mouseY - prevMouseY;

                                     var fovY = this.viewerImpl_.FovY;
                                     var fovX = fovY / this.Height * this.Width;

                                     var deltaXFrac
                                         = 1f * deltaMouseX / this.Width;
                                     var deltaYFrac
                                         = 1f * deltaMouseY / this.Height;

                                     var mouseSpeed = 3;

                                     this.Camera.PitchDegrees = float.Clamp(
                                         this.Camera.PitchDegrees -
                                         deltaYFrac * fovY * mouseSpeed,
                                         -90,
                                         90);
                                     this.Camera.YawDegrees
                                         -= deltaXFrac * fovX * mouseSpeed;
                                   }

                                   this.prevMousePosition_ = mouseLocation;
                                 }
                               };

      inputTarget.KeyDown += (_, args) => {
                               switch (args.KeyCode) {
                                 case Keys.W: {
                                   this.isForwardDown_ = true;
                                   break;
                                 }
                                 case Keys.S: {
                                   this.isBackwardDown_ = true;
                                   break;
                                 }
                                 case Keys.A: {
                                   this.isLeftwardDown_ = true;
                                   break;
                                 }
                                 case Keys.D: {
                                   this.isRightwardDown_ = true;
                                   break;
                                 }
                                 case Keys.Q: {
                                   this.isDownwardDown_ = true;
                                   break;
                                 }
                                 case Keys.E: {
                                   this.isUpwardDown_ = true;
                                   break;
                                 }
                                 case Keys.ShiftKey: {
                                   this.isSpeedupActive_ = true;
                                   break;
                                 }
                                 case Keys.ControlKey: {
                                   this.isSlowdownActive_ = true;
                                   break;
                                 }
                               }
                             };

      inputTarget.KeyUp += (_, args) => {
                             switch (args.KeyCode) {
                               case Keys.W: {
                                 this.isForwardDown_ = false;
                                 break;
                               }
                               case Keys.S: {
                                 this.isBackwardDown_ = false;
                                 break;
                               }
                               case Keys.A: {
                                 this.isLeftwardDown_ = false;
                                 break;
                               }
                               case Keys.D: {
                                 this.isRightwardDown_ = false;
                                 break;
                               }
                               case Keys.Q: {
                                 this.isDownwardDown_ = false;
                                 break;
                               }
                               case Keys.E: {
                                 this.isUpwardDown_ = false;
                                 break;
                               }
                               case Keys.ShiftKey: {
                                 this.isSpeedupActive_ = false;
                                 break;
                               }
                               case Keys.ControlKey: {
                                 this.isSlowdownActive_ = false;
                                 break;
                               }
                             }
                           };
    }

  protected override void InitGl() => GlUtil.InitGl();

  protected override void RenderGl() {
      var forwardVector =
          (this.isForwardDown_ ? 1 : 0) - (this.isBackwardDown_ ? 1 : 0);
      var rightwardVector =
          (this.isRightwardDown_ ? 1 : 0) - (this.isLeftwardDown_ ? 1 : 0);
      var upwardVector =
          (this.isUpwardDown_ ? 1 : 0) - (this.isDownwardDown_ ? 1 : 0);

      var cameraSpeed = UiConstants.GLOBAL_SCALE * 15;
      if (this.isSpeedupActive_) {
        cameraSpeed *= 2;
      }

      if (this.isSlowdownActive_) {
        cameraSpeed /= 2;
      }

      this.Camera.Move(forwardVector,
                       rightwardVector,
                       upwardVector,
                       cameraSpeed);

      this.viewerImpl_.Width = this.Width;
      this.viewerImpl_.Height = this.Height;
      this.viewerImpl_.GlobalScale = UiConstants.GLOBAL_SCALE;
      this.viewerImpl_.NearPlane = UiConstants.NEAR_PLANE;
      this.viewerImpl_.FarPlane = UiConstants.FAR_PLANE;
      this.viewerImpl_.ShowGrid = Config.Instance.Viewer.ShowGrid;

      this.viewerImpl_.Render();
    }

  public ISceneInstance? Scene {
    get => this.viewerImpl_.Scene;
    set {
        this.viewerImpl_.Scene = value;

        if (value == null) {
          this.viewerImpl_.ViewerScale = 1;
        } else {
          this.viewerImpl_.ViewerScale =
              new ScaleSource(Config.Instance.Viewer
                                    .ViewerModelScaleSource).GetScale(value);
        }
      }
  }

  public IAnimatableModel? FirstSceneModel
    => this.viewerImpl_.FirstSceneModel;

  public IAnimationPlaybackManager? AnimationPlaybackManager
    => this.viewerImpl_.AnimationPlaybackManager;

  public IReadOnlyModelAnimation? Animation {
    get => this.viewerImpl_.Animation;
    set => this.viewerImpl_.Animation = value;
  }

  public ISkeletonRenderer? SkeletonRenderer
    => this.viewerImpl_.SkeletonRenderer;

  public Camera Camera => this.viewerImpl_.Camera;
}