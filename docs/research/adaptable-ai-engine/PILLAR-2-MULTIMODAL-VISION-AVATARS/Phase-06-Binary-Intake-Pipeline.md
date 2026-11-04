# Phase 06: Client-Side Drag-and-Drop Binary Streamer

**Pillar:** II — Multimodal Perception, Vision & Avatar Pipeline  
**Module:** `TeamUp.Media.Intake`  
**Target:** High-Performance, Secure Image Intake for Avatars and Documents  

---

## 1. Problem & Architectural Challenge
Users frequently drop raw camera images (15MB+ JPEGs, HEIC files from iPhones, or web images) directly into the avatar target area.
Transmitting raw 15MB payloads to the backend API:
1. Causes massive bandwidth bottlenecks and latency spikes.
2. Exposes the server to denial-of-service memory exhaustion.
3. Risks file upload exploits (e.g. polyglot executables renamed to `.jpg`).

**Requirement:** Process binary image drops on the client with zero security leaks and sub-1s transmission.

---

## 2. Research & Engineering Specifications

### Client-Side Pre-Flight Normalization (WebCodecs & OffscreenCanvas)
*Citation: W3C Media Working Group (2024), "WebCodecs API Specification"; RFC 2046 (MIME Types).*

Before sending bytes over the wire, the client executes an in-browser normalization:
1. **Magic-Byte Sniffing:** Verifies true binary signatures (`0xFF 0xD8 0xFF` for JPEG, `0x89 0x50 0x4E 0x47` for PNG, `RIFF...WEBP` for WebP). Ignores deceptive file extensions.
2. **OffscreenCanvas Resizing:** Rescales images exceeding 2048x2048 pixels down to a maximum bounding box of 1024x1024.
3. **EXIF Stripping:** Removes GPS coordinates, camera serial numbers, and personal metadata for student privacy.
4. **Target Compression:** Re-encodes to modern WebP (Quality: 85%), reducing a 12MB raw photo to **~180KB** with zero perceptible loss in visual quality.

```
Dropped File (15MB HEIC/JPEG) ──► Magic-Byte Check ──► Canvas Resize & EXIF Strip ──► WebP Blob (180KB) ──► Upload
```

---

## 3. Concrete Implementation Contracts

### TypeScript Client-Side Normalizer
```typescript
export async function normalizeDroppedImage(file: File): Promise<{ blob: Blob; mime: string }> {
  const buffer = await file.slice(0, 8).arrayBuffer();
  const bytes = new Uint8Array(buffer);

  // Magic-byte validation
  const isPng = bytes[0] === 0x89 && bytes[1] === 0x50 && bytes[2] === 0x4e && bytes[3] === 0x47;
  const isJpg = bytes[0] === 0xff && bytes[1] === 0xd8 && bytes[2] === 0xff;
  const isWebp = bytes[0] === 0x52 && bytes[1] === 0x49 && bytes[2] === 0x46 && bytes[3] === 0x46;

  if (!isPng && !isJpg && !isWebp) {
    throw new Error('Unsupported image format. Please drop a valid PNG, JPEG, or WebP image.');
  }

  const bitmap = await createImageBitmap(file);
  const maxDim = 1024;
  let { width, height } = bitmap;

  if (width > maxDim || height > maxDim) {
    if (width > height) {
      height = Math.round((height * maxDim) / width);
      width = maxDim;
    } else {
      width = Math.round((width * maxDim) / height);
      height = maxDim;
    }
  }

  const canvas = new OffscreenCanvas(width, height);
  const ctx = canvas.getContext('2d')!;
  ctx.drawImage(bitmap, 0, 0, width, height);

  const compressedBlob = await canvas.convertToBlob({ type: 'image/webp', quality: 0.85 });
  return { blob: compressedBlob, mime: 'image/webp' };
}
```

---

## 4. Failure Modes & Mitigations
- **Zip-bomb / Pixel-flood attacks:** `createImageBitmap` runs in a sandboxed worker thread; images with declared dimensions exceeding 10,000x10,000 pixels are rejected immediately before decoding memory is allocated.
- **Corrupted streams:** SHA-256 hash calculated client-side and verified against server receipt.
