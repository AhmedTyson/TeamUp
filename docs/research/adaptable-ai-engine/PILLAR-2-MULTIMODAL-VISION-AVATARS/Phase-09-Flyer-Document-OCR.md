# Phase 09: Flyer, Syllabus & Document OCR Semantic Extraction

**Pillar:** II — Multimodal Perception, Vision & Avatar Pipeline  
**Module:** `TeamUp.Media.DocumentOcr`  
**Target:** Converting Unstructured Posters, Flyers & Documents into Structured Entities  

---

## 1. Problem & Architectural Challenge
Student organizations rarely fill out software forms. They design a flyer in Canva, export a `.png` or `.pdf`, and post it on social media:
> *"2-Day Machine Learning Hackathon! Nov 20-21, 9 AM - 5 PM. Room 304, FCAI. Prerequisites: Python, Git. Max teams: 4 per group."*

Expecting an event organizer to re-enter this information into 15 form fields leads to low platform adoption and stale event boards.

**Requirement:** The organizer drags the flyer into the platform. The system extracts the event schedule, prerequisites, venue, and tracks with **zero manual typing**, presenting a pre-filled draft for 1-click confirmation.

---

## 2. Research & Multimodal Parsing Pipeline

### Vision-Language Document Understanding vs. Traditional OCR
*Citation: Kim et al. (NAACL 2022), "OCR-free Document Understanding Transformer (Donut)"; Blecher et al. (2024), "Nougat: Neural Optical Understanding for Academic Documents".*

Traditional pipeline: `Tesseract OCR ➔ Raw Unstructured Text ➔ LLM Parser`.  
**Flaw:** Loses 2D spatial layout, font hierarchy, date badges, and visual groupings.

Modern pipeline: **End-to-End Multimodal Vision-Language Model** (Gemini 1.5/2.0 Pro Vision / GPT-4o Vision):
The model receives the raw pixel tensor directly, preserving:
- Bold titles and visual hierarchy (determining `Event.Title`).
- Date icons and timetable columns (determining `EventSession.StartTime` & `EndTime`).
- Partner/club logos (determining `Organization.Name`).
- Pill badges (determining `RequiredSkills`).

```
Flyer Image (2D Pixels) ──► Vision-Language Model + Structured Grammar ──► EventDraftDto
```

---

## 3. Concrete Schema & Extraction Implementation

### Target Event Draft Schema
```json
{
  "title": "Machine Learning Workshop 2026",
  "description": "Comprehensive two-day intensive workshop covering Deep Learning, CNNs, and PyTorch fundamentals.",
  "type": "Workshop",
  "location": "Room 304, Faculty of Computers and AI",
  "startDate": "2026-11-20T09:00:00Z",
  "endDate": "2026-11-21T17:00:00Z",
  "capacity": 50,
  "sessions": [
    {
      "sessionNo": 1,
      "title": "Day 1: Foundations of Deep Learning",
      "startTime": "2026-11-20T09:00:00Z",
      "endTime": "2026-11-20T17:00:00Z",
      "room": "Room 304"
    },
    {
      "sessionNo": 2,
      "title": "Day 2: Computer Vision & Model Deployment",
      "startTime": "2026-11-21T09:00:00Z",
      "endTime": "2026-11-21T17:00:00Z",
      "room": "Room 304"
    }
  ],
  "extractedSkills": ["Python", "PyTorch", "Git"],
  "confidence": 0.94
}
```

### Multimodal Extraction Service (C#)
```csharp
namespace TeamUp.Application.Ai.Extraction;

public class FlyerExtractionService(IMultimodalLlmClient llmClient) : IFlyerExtractionService
{
    private const string SystemPrompt = """
        You are an expert event extractor. Given an event flyer image, extract all event details strictly adhering
        to the provided JSON schema. Ensure dates are parsed to ISO-8601 UTC. If capacity or room is not explicitly
        stated, set them to null. Do not hallucinate sponsors or dates not present on the image.
        """;

    public async Task<EventDraftDto> ExtractFromFlyerAsync(byte[] imageBytes, string mimeType, CancellationToken ct)
    {
        var result = await llmClient.ExtractStructuredJsonAsync<EventDraftDto>(
            imageBytes, mimeType, SystemPrompt, ct);

        // Deterministic date chronological sanity check
        if (result.EndDate < result.StartDate)
            throw new ValidationException("Extracted end date is earlier than start date.");

        return result;
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Relative Dates ("Next Friday"):** System prompt injects `CurrentUtcDate` context into the vision prompt so the model calculates absolute ISO timestamps accurately.
- **Low-Resolution / Blurry Flyers:** If text confidence score is < 0.70, the extractor returns a partial draft with prominent input warnings on low-confidence fields.
