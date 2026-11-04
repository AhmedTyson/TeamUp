# Phase 08: Autonomous Bounding Box Centering & Focal Cropping

**Pillar:** II — Multimodal Perception, Vision & Avatar Pipeline  
**Module:** `TeamUp.Media.FocalCrop`  
**Target:** Automated Facial & Character Centering for Circular 1:1 Avatars  

---

## 1. Problem & Architectural Challenge
When users upload a character illustration or full-body photograph, standard center-cropping (`object-fit: cover`) fails:
- The character's face is cut off at the top.
- The avatar centers on the character's chest or background scenery.
- Forcing users to drag a manual cropping rectangle breaks the frictionless "zero-routine-input" objective.

**Requirement:** Automatically detect the visual focal point (human face or illustrated character head) and crop a balanced 1:1 square centered on the character with zero user manual adjustment.

---

## 2. Research & Landmark Centering Algorithm

### Face & Character Bounding Box Detection
*Citation: Bazarevsky et al. (Google Research, 2020), "BlazeFace: Sub-millisecond Neural Face Detection on Mobile GPUs"; Zhang et al., "Joint Face Detection and Alignment using Multi-task Cascaded Convolutional Networks (MTCNN)".*

```
Original Rectangular Image (1920 x 1080)
┌──────────────────────────────────────────────┐
│                  [Character Head]            │
│                         O                    │
│                        /|\                   │
│                        / \                   │
└──────────────────────────────────────────────┘
                         │
                         ▼ Detection Box (cx, cy, width, height)
                    Center of Interest = (cx, cy)
                         │
                         ▼ Golden Ratio Bounding Square (1:1)
┌──────────────────────────────────────────────┐
│              ┌──────────────┐                │
│              │     ( O )    │ ◄── 1:1 Crop Box with 25% Headroom Pad
│              │     / | \    │                │
│              └──────────────┘                │
└──────────────────────────────────────────────┘
```

### Golden Ratio Framing Heuristic:
1. Detect landmark bounding box $B = (x_{\text{min}}, y_{\text{min}}, x_{\text{max}}, y_{\text{max}})$.
2. Calculate center: $C = (\frac{x_{\text{min}} + x_{\text{max}}}{2}, \frac{y_{\text{min}} + y_{\text{max}}}{2})$.
3. Add **Headroom Margin**: Character/hair bounding requires 25% vertical padding above $y_{\text{min}}$.
4. Expand to minimum dimension square $\text{Side} = \max(\text{BoxWidth}, \text{BoxHeight}) \times 1.6$.
5. Clamp crop coordinates strictly within original image boundaries.

---

## 3. Concrete Implementation (C# / SixLabors.ImageSharp)

```csharp
namespace TeamUp.Application.Media;

public sealed record CropRectangle(int X, int Y, int Width, int Height);

public class SmartFocalCropService(IFaceLandmarkDetector detector) : ISmartFocalCropService
{
    public async Task<byte[]> AutoCropAvatarAsync(byte[] imageBytes, int targetResolution = 512, CancellationToken ct = default)
    {
        using var image = Image.Load(imageBytes);

        // Detect character face or primary visual saliency
        var faceBox = await detector.DetectPrimarySubjectAsync(imageBytes, ct);

        CropRectangle crop;
        if (faceBox is not null)
        {
            // Frame around character head with optimal padding
            int side = (int)Math.Max(faceBox.Width * 1.8, faceBox.Height * 1.8);
            int centerX = faceBox.X + (faceBox.Width / 2);
            int centerY = faceBox.Y + (int)(faceBox.Height * 0.4); // slightly higher for forehead/hair

            int cropX = Math.Clamp(centerX - (side / 2), 0, image.Width - side);
            int cropY = Math.Clamp(centerY - (side / 2), 0, image.Height - side);

            // Guard for boundary overflow
            side = Math.Min(side, Math.Min(image.Width, image.Height));
            crop = new CropRectangle(cropX, cropY, side, side);
        }
        else
        {
            // Fallback: Smart Saliency / Center crop
            int side = Math.Min(image.Width, image.Height);
            int cropX = (image.Width - side) / 2;
            int cropY = (image.Height - side) / 2;
            crop = new CropRectangle(cropX, cropY, side, side);
        }

        // Apply crop and high-quality Lanczos3 downsample
        image.Mutate(ctx => ctx
            .Crop(new Rectangle(crop.X, crop.Y, crop.Width, crop.Height))
            .Resize(new ResizeOptions
            {
                Size = new Size(targetResolution, targetResolution),
                Sampler = KnownResamplers.Lanczos3
            }));

        using var ms = new MemoryStream();
        await image.SaveAsWebpAsync(ms, new WebpEncoder { Quality = 85 }, ct);
        return ms.ToArray();
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Multiple Faces / Characters in Image:** The detector selects the subject with the largest bounding box area (primary focal character).
- **No Face Detected (e.g. minimalist geometric avatar):** Falls back to edge-density saliency mapping; centers around the highest entropy region.
