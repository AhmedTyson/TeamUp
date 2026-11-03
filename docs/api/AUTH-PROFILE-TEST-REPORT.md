# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 21:54:52 UTC
> **Target Environment:** ASP.NET Core 10 Web API (http://localhost:5199)
> **Database Engine:** Microsoft SQL Server 2022 (Docker container 	eamup-sql:14333)
> **Architecture Standards:** RFC 7807 ProblemDetails, JWT Bearer Token, Single-Column Soft Delete (DeletedAt), BaseResponse<T> Envelope
> **Payload Ordering Rule:** Personal identity properties first (irstName, lastName, mail, password), followed by academic and institutional affiliations.

---

### Test Case: 01. Service Health Check

**Request:**
```http
GET /health HTTP/1.1
Host: localhost:5199
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "status": "Healthy"
}
```

---

### Test Case: 02. Student Registration (Happy Path)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "firstName": "Ahmed",
  "lastName": "Tyson",
  "email": "live_student_267034051@teamup.edu",
  "password": "StrongP@ssw0rd2026!",
  "academicYear": 3,
  "facultyId": 1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJhYTE1NjUzZC05NTdiLTQwZGItOGRhNC1jNjhmNDIyMTViYmQiLCJleHAiOjE3OTM3MzkyOTYsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.J3hfxXroz6Dr2dS5mG-eSqSTFUVnFK-9qxPGFe5vQAM",
    "refreshToken": "orylpElHKPOiiuZvyNZ0X\u002B/jxSMR455FfQmYBlX5aXV2Xbjk\u002Bw6tkY69\u002BxcfmlnZkE6l/UVEW5kmwxE1JgjYdQ==",
    "expiresAt": "2026-11-03T20:54:56.2207067Z",
    "student": {
      "studentId": 14,
      "firstName": "Ahmed",
      "lastName": "Tyson",
      "email": "live_student_267034051@teamup.edu",
      "academicYear": 3,
      "role": "Student",
      "facultyId": 1,
      "facultyName": "Faculty of Computers and Artificial Intelligence",
      "universityName": "Cairo University"
    }
  },
  "success": true,
  "message": "Student registered successfully.",
  "statusCode": 201,
  "timestamp": "2026-11-03T19:54:56.2488215Z"
}
```

---

### Test Case: 03. Registration Conflict (Duplicate Email)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "firstName": "Duplicate",
  "lastName": "User",
  "email": "live_student_267034051@teamup.edu",
  "password": "AnotherPassword123!",
  "academicYear": 2,
  "facultyId": 1
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  49,
  48,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  67,
  111,
  110,
  102,
  108,
  105,
  99,
  116,
  32,
  68,
  101,
  116,
  101,
  99,
  116,
  101,
  100,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  57,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  65,
  32,
  115,
  116,
  117,
  100,
  101,
  110,
  116,
  32,
  119,
  105,
  116,
  104,
  32,
  116,
  104,
  105,
  115,
  32,
  101,
  109,
  97,
  105,
  108,
  32,
  97,
  100,
  100,
  114,
  101,
  115,
  115,
  32,
  97,
  108,
  114,
  101,
  97,
  100,
  121,
  32,
  101,
  120,
  105,
  115,
  116,
  115,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  114,
  101,
  103,
  105,
  115,
  116,
  101,
  114,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  100,
  56,
  49,
  101,
  101,
  54,
  48,
  97,
  49,
  50,
  98,
  53,
  100,
  56,
  55,
  48,
  49,
  97,
  48,
  50,
  99,
  99,
  57,
  57,
  51,
  51,
  52,
  98,
  49,
  51,
  52,
  101,
  45,
  53,
  51,
  49,
  53,
  51,
  57,
  102,
  48,
  56,
  53,
  97,
  52,
  52,
  52,
  54,
  51,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 04. Registration Validation (Empty Password)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "firstName": "Empty",
  "lastName": "Password",
  "email": "invalid_user@teamup.edu",
  "password": "",
  "academicYear": 1,
  "facultyId": 1
}
```

**Response:** `400 BadRequest` — **🟢 PASSED** (Expected: `400`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/auth/register",
  "errors": [
    {
      "field": "password",
      "message": "Password is required."
    },
    {
      "field": "password",
      "message": "Password must be at least 8 characters long."
    },
    {
      "field": "password",
      "message": "Password must contain at least one uppercase letter."
    },
    {
      "field": "password",
      "message": "Password must contain at least one lowercase letter."
    },
    {
      "field": "password",
      "message": "Password must contain at least one digit."
    }
  ],
  "timestamp": "2026-11-03T19:54:56.4920681Z"
}
```

---

### Test Case: 05. Registration ForeignKey Failure (Invalid Faculty ID)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "firstName": "Ghost",
  "lastName": "Faculty",
  "email": "bad_faculty_883367579@teamup.edu",
  "password": "ValidPassword123!",
  "academicYear": 1,
  "facultyId": 99999
}
```

**Response:** `404 NotFound` — **🟢 PASSED** (Expected: `404`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  53,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  82,
  101,
  115,
  111,
  117,
  114,
  99,
  101,
  32,
  78,
  111,
  116,
  32,
  70,
  111,
  117,
  110,
  100,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  52,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  70,
  97,
  99,
  117,
  108,
  116,
  121,
  32,
  119,
  105,
  116,
  104,
  32,
  73,
  68,
  32,
  57,
  57,
  57,
  57,
  57,
  32,
  119,
  97,
  115,
  32,
  110,
  111,
  116,
  32,
  102,
  111,
  117,
  110,
  100,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  114,
  101,
  103,
  105,
  115,
  116,
  101,
  114,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  53,
  102,
  101,
  98,
  102,
  48,
  49,
  54,
  100,
  98,
  97,
  55,
  99,
  101,
  98,
  57,
  100,
  49,
  100,
  50,
  53,
  54,
  98,
  50,
  50,
  97,
  56,
  101,
  98,
  98,
  55,
  57,
  45,
  51,
  99,
  53,
  48,
  49,
  99,
  99,
  97,
  53,
  98,
  50,
  53,
  101,
  54,
  100,
  53,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 06. Student Login (Valid Credentials)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "live_student_267034051@teamup.edu",
  "password": "StrongP@ssw0rd2026!"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiIzNzE3NzNkMS1hOWJjLTQ3M2QtYmJiZi0wMzkzY2RlZmI2NzUiLCJleHAiOjE3OTM3MzkyOTYsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.pba3Ae1-NcrqjYiDKjzoCDdZUz7I9Nh0Boknbw9do94",
    "refreshToken": "R\u002Bm2/tZv6ZInEJTQZaz2KZFgri9m0zJ5xvhYCx3T96R2Bg\u002BWXMs\u002BvACeA0zJNGOqdIXeyuskdDb0WNALVMCNrA==",
    "expiresAt": "2026-11-03T20:54:56.7951231Z",
    "student": {
      "studentId": 14,
      "firstName": "Ahmed",
      "lastName": "Tyson",
      "email": "live_student_267034051@teamup.edu",
      "academicYear": 3,
      "role": "Student",
      "facultyId": 1,
      "facultyName": "Faculty of Computers and Artificial Intelligence",
      "universityName": "Cairo University"
    }
  },
  "success": true,
  "message": "Login successful.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:56.8354072Z"
}
```

---

### Test Case: 07. Login Authentication Failure (Incorrect Password)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "live_student_267034051@teamup.edu",
  "password": "WrongPasswordXYZ!"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  50,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  85,
  110,
  97,
  117,
  116,
  104,
  111,
  114,
  105,
  122,
  101,
  100,
  32,
  65,
  99,
  99,
  101,
  115,
  115,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  49,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  73,
  110,
  118,
  97,
  108,
  105,
  100,
  32,
  101,
  109,
  97,
  105,
  108,
  32,
  111,
  114,
  32,
  112,
  97,
  115,
  115,
  119,
  111,
  114,
  100,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  108,
  111,
  103,
  105,
  110,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  55,
  98,
  55,
  100,
  97,
  52,
  101,
  53,
  53,
  49,
  50,
  51,
  99,
  54,
  56,
  98,
  48,
  52,
  100,
  98,
  51,
  49,
  51,
  50,
  48,
  97,
  97,
  53,
  97,
  98,
  57,
  53,
  45,
  52,
  53,
  57,
  100,
  98,
  101,
  50,
  56,
  53,
  99,
  98,
  52,
  54,
  57,
  57,
  53,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 08. Login Authentication Failure (Unknown Email)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "does_not_exist@teamup.edu",
  "password": "AnyPassword123!"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  50,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  85,
  110,
  97,
  117,
  116,
  104,
  111,
  114,
  105,
  122,
  101,
  100,
  32,
  65,
  99,
  99,
  101,
  115,
  115,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  49,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  73,
  110,
  118,
  97,
  108,
  105,
  100,
  32,
  101,
  109,
  97,
  105,
  108,
  32,
  111,
  114,
  32,
  112,
  97,
  115,
  115,
  119,
  111,
  114,
  100,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  108,
  111,
  103,
  105,
  110,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  57,
  102,
  55,
  55,
  52,
  53,
  57,
  50,
  54,
  51,
  48,
  102,
  101,
  98,
  99,
  102,
  56,
  98,
  101,
  56,
  102,
  100,
  53,
  54,
  57,
  101,
  53,
  49,
  49,
  53,
  56,
  51,
  45,
  100,
  99,
  99,
  101,
  57,
  100,
  55,
  97,
  50,
  52,
  50,
  51,
  101,
  51,
  55,
  55,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 09. Refresh Access Token (Token Rotation)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "refreshToken": "R+m2/tZv6ZInEJTQZaz2KZFgri9m0zJ5xvhYCx3T96R2Bg+WXMs+vACeA0zJNGOqdIXeyuskdDb0WNALVMCNrA=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI",
    "refreshToken": "fcQ55onUXpqwyFy9iHHJDJD/oq2CywxzYSjF3rPR5mR4/lYJm9lHnE0q2S3QR63S5KeWiFpR3VBXUxbmAsQQ/Q==",
    "expiresAt": "2026-11-03T20:54:57.0347407Z",
    "student": {
      "studentId": 14,
      "firstName": "Ahmed",
      "lastName": "Tyson",
      "email": "live_student_267034051@teamup.edu",
      "academicYear": 3,
      "role": "Student",
      "facultyId": 1,
      "facultyName": "Faculty of Computers and Artificial Intelligence",
      "universityName": "Cairo University"
    }
  },
  "success": true,
  "message": "Token refreshed successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.0446255Z"
}
```

---

### Test Case: 10. Refresh Token Failure (Tampered/Invalid Token)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "refreshToken": "TAMPERED_REFRESH_TOKEN_1234567890_INVALID"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  50,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  85,
  110,
  97,
  117,
  116,
  104,
  111,
  114,
  105,
  122,
  101,
  100,
  32,
  65,
  99,
  99,
  101,
  115,
  115,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  49,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  73,
  110,
  118,
  97,
  108,
  105,
  100,
  32,
  111,
  114,
  32,
  101,
  120,
  112,
  105,
  114,
  101,
  100,
  32,
  114,
  101,
  102,
  114,
  101,
  115,
  104,
  32,
  116,
  111,
  107,
  101,
  110,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  114,
  101,
  102,
  114,
  101,
  115,
  104,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  101,
  50,
  51,
  50,
  54,
  100,
  102,
  56,
  53,
  55,
  56,
  102,
  99,
  53,
  54,
  49,
  51,
  97,
  48,
  100,
  54,
  97,
  101,
  98,
  54,
  101,
  55,
  102,
  52,
  56,
  48,
  51,
  45,
  54,
  48,
  57,
  52,
  50,
  52,
  52,
  57,
  55,
  102,
  97,
  51,
  54,
  53,
  49,
  56,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 11. Protected Route Guard (Missing Authorization Header)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  50,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  85,
  110,
  97,
  117,
  116,
  104,
  111,
  114,
  105,
  122,
  101,
  100,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  49,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  97,
  98,
  52,
  48,
  100,
  54,
  100,
  56,
  50,
  52,
  53,
  56,
  50,
  50,
  98,
  49,
  55,
  51,
  57,
  56,
  57,
  101,
  100,
  53,
  50,
  57,
  102,
  56,
  101,
  56,
  52,
  101,
  45,
  52,
  49,
  52,
  54,
  48,
  55,
  98,
  51,
  48,
  51,
  99,
  55,
  97,
  48,
  99,
  49,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 14,
    "firstName": "Ahmed",
    "lastName": "Tyson",
    "email": "live_student_267034051@teamup.edu",
    "academicYear": 3,
    "role": "Student",
    "facultyId": 1,
    "facultyName": "Faculty of Computers and Artificial Intelligence",
    "universityName": "Cairo University",
    "profile": null,
    "interests": []
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.340826Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
Content-Type: application/json

{
  "firstName": "Ahmed (Lead)",
  "lastName": "Tyson",
  "academicYear": 4
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 14,
    "firstName": "Ahmed (Lead)",
    "lastName": "Tyson",
    "email": "live_student_267034051@teamup.edu",
    "academicYear": 4,
    "role": "Student",
    "facultyId": 1,
    "facultyName": "Faculty of Computers and Artificial Intelligence",
    "universityName": "Cairo University",
    "profile": null,
    "interests": []
  },
  "success": true,
  "message": "Student account updated successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.4297284Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/14/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "bio": null,
    "photoUrl": null,
    "gitHubUrl": null,
    "linkedInUrl": null
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.4683405Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
Content-Type: application/json

{
  "bio": "Full-stack engineer building TeamUp. Spec-Driven & Test-Driven Development advocate.",
  "photoUrl": "https://avatars.githubusercontent.com/u/AhmedTyson",
  "gitHubUrl": "https://github.com/AhmedTyson",
  "linkedInUrl": "https://linkedin.com/in/ahmedtyson"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "bio": "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl": "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl": "https://github.com/AhmedTyson",
    "linkedInUrl": "https://linkedin.com/in/ahmedtyson"
  },
  "success": true,
  "message": "Profile updated successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.5639385Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/14/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "bio": "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl": "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl": "https://github.com/AhmedTyson",
    "linkedInUrl": "https://linkedin.com/in/ahmedtyson"
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.5768252Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
Content-Type: application/json

{
  "interest": "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "Interest added successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.6483165Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
Content-Type: application/json

{
  "interest": "Cloud & Distributed Systems"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "Interest added successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.6708486Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/14/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [
    "Artificial Intelligence",
    "Cloud \u0026 Distributed Systems"
  ],
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.7109032Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/14/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [
    "Cloud \u0026 Distributed Systems"
  ],
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.7708603Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
Content-Type: application/json

{
  "interest": "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "Interest added successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.7916464Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/14/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [
    "Artificial Intelligence",
    "Cloud \u0026 Distributed Systems"
  ],
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:57.8024678Z"
}
```

---

### Test Case: 24. PlatformAdmin Authentication

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "admin@teamup.local",
  "password": "Adm!0RK8zvFIO2BDTX"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY",
    "refreshToken": "U0JWlKlnHD1\u002B9PT0wSTvsvzSaVGceXM8eb16k1OV4IZr0PTq4SFficxs2RG1C/PoqvLX\u002BXh/RDi27ffFjzP7pw==",
    "expiresAt": "2026-11-03T20:54:59.9342196Z",
    "student": {
      "studentId": 1,
      "firstName": "Platform",
      "lastName": "Admin",
      "email": "admin@teamup.local",
      "academicYear": 1,
      "role": "PlatformAdmin",
      "facultyId": 1,
      "facultyName": "Faculty of Computers and Artificial Intelligence",
      "universityName": "Cairo University"
    }
  },
  "success": true,
  "message": "Login successful.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:54:59.9494643Z"
}
```

---

### Test Case: 24B. Public Academic Catalog Lookup for Registration (GET /universities/catalog)

**Request:**
```http
GET /api/v1/universities/catalog HTTP/1.1
Host: localhost:5199
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [
    {
      "universityId": 6,
      "name": "Alexandria University 2078208904 Updated",
      "city": "Alexandria",
      "country": "Egypt",
      "website": "https://updated.alexu.edu.eg",
      "faculties": [
        {
          "facultyId": 7,
          "name": "Faculty of Medicine \u0026 Health",
          "code": "MED_HEALTH"
        }
      ]
    },
    {
      "universityId": 1,
      "name": "Cairo University",
      "city": "Giza",
      "country": "Egypt",
      "website": "https://cu.edu.eg",
      "faculties": [
        {
          "facultyId": 1,
          "name": "Faculty of Computers and Artificial Intelligence",
          "code": "FCAI"
        },
        {
          "facultyId": 2,
          "name": "Faculty of Engineering",
          "code": "ENG"
        }
      ]
    }
  ],
  "success": true,
  "message": "Academic catalog retrieved successfully for registration lookups.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.0364436Z"
}
```

---

### Test Case: 25. List Universities with Paging (GET /universities)

**Request:**
```http
GET /api/v1/universities?page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "items": [
      {
        "universityId": 6,
        "name": "Alexandria University 2078208904 Updated",
        "city": "Alexandria",
        "country": "Egypt",
        "website": "https://updated.alexu.edu.eg",
        "facultyCount": 1
      },
      {
        "universityId": 1,
        "name": "Cairo University",
        "city": "Giza",
        "country": "Egypt",
        "website": "https://cu.edu.eg",
        "facultyCount": 2
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 2,
    "totalPages": 1
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.1415971Z"
}
```

---

### Test Case: 26. Get University Details with Faculties (GET /universities/{id})

**Request:**
```http
GET /api/v1/universities/1 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "universityId": 1,
    "name": "Cairo University",
    "city": "Giza",
    "country": "Egypt",
    "website": "https://cu.edu.eg",
    "faculties": [
      {
        "facultyId": 1,
        "name": "Faculty of Computers and Artificial Intelligence",
        "code": "FCAI"
      },
      {
        "facultyId": 2,
        "name": "Faculty of Engineering",
        "code": "ENG"
      }
    ]
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.2084681Z"
}
```

---

### Test Case: 27. Create University as Student Forbidden (Role Guard Check)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
Content-Type: application/json

{
  "name": "Student Unauthorized University",
  "city": "Cairo",
  "country": "Egypt"
}
```

**Response:** `403 Forbidden` — **🟢 PASSED** (Expected: `403`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  52,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  70,
  111,
  114,
  98,
  105,
  100,
  100,
  101,
  110,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  51,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  50,
  55,
  97,
  54,
  100,
  98,
  56,
  56,
  55,
  100,
  101,
  100,
  55,
  102,
  48,
  100,
  48,
  54,
  55,
  55,
  55,
  57,
  100,
  99,
  50,
  53,
  52,
  102,
  49,
  53,
  52,
  51,
  45,
  101,
  100,
  56,
  98,
  55,
  56,
  49,
  50,
  48,
  48,
  98,
  56,
  51,
  99,
  101,
  49,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 28. Create Accredited University as Admin (POST /universities)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "name": "Alexandria University 6446711",
  "city": "Alexandria",
  "country": "Egypt",
  "website": "https://alexu.edu.eg"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
  "data": {
    "universityId": 7,
    "name": "Alexandria University 6446711",
    "city": "Alexandria",
    "country": "Egypt",
    "website": "https://alexu.edu.eg",
    "faculties": []
  },
  "success": true,
  "message": "University created successfully.",
  "statusCode": 201,
  "timestamp": "2026-11-03T19:55:00.2853128Z"
}
```

---

### Test Case: 29. Create University Conflict (Duplicate Name)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "name": "Alexandria University 6446711",
  "city": "Alexandria",
  "country": "Egypt"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  49,
  48,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  67,
  111,
  110,
  102,
  108,
  105,
  99,
  116,
  32,
  68,
  101,
  116,
  101,
  99,
  116,
  101,
  100,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  57,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  65,
  32,
  117,
  110,
  105,
  118,
  101,
  114,
  115,
  105,
  116,
  121,
  32,
  110,
  97,
  109,
  101,
  100,
  32,
  39,
  65,
  108,
  101,
  120,
  97,
  110,
  100,
  114,
  105,
  97,
  32,
  85,
  110,
  105,
  118,
  101,
  114,
  115,
  105,
  116,
  121,
  32,
  54,
  52,
  52,
  54,
  55,
  49,
  49,
  39,
  32,
  97,
  108,
  114,
  101,
  97,
  100,
  121,
  32,
  101,
  120,
  105,
  115,
  116,
  115,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  117,
  110,
  105,
  118,
  101,
  114,
  115,
  105,
  116,
  105,
  101,
  115,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  56,
  54,
  50,
  48,
  53,
  54,
  97,
  55,
  52,
  56,
  99,
  97,
  101,
  101,
  53,
  99,
  98,
  51,
  101,
  56,
  101,
  101,
  100,
  51,
  49,
  54,
  98,
  55,
  51,
  50,
  102,
  51,
  45,
  50,
  98,
  48,
  57,
  97,
  49,
  50,
  98,
  54,
  101,
  49,
  98,
  102,
  56,
  50,
  48,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 30. Update University Information (PUT /universities/{id})

**Request:**
```http
PUT /api/v1/universities/7 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "name": "Alexandria University 6446711 Updated",
  "city": "Alexandria",
  "country": "Egypt",
  "website": "https://updated.alexu.edu.eg"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "universityId": 7,
    "name": "Alexandria University 6446711 Updated",
    "city": "Alexandria",
    "country": "Egypt",
    "website": "https://updated.alexu.edu.eg",
    "faculties": []
  },
  "success": true,
  "message": "University updated successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.4326317Z"
}
```

---

### Test Case: 31. List Faculties in University (GET /universities/{id}/faculties)

**Request:**
```http
GET /api/v1/universities/7/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [],
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.4957232Z"
}
```

---

### Test Case: 32. Add Faculty to University as Admin (POST /universities/{id}/faculties)

**Request:**
```http
POST /api/v1/universities/7/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "name": "Faculty of Medicine",
  "code": "MED_ALEX"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
  "data": {
    "facultyId": 8,
    "name": "Faculty of Medicine",
    "code": "MED_ALEX",
    "universityId": 7,
    "universityName": "Alexandria University 6446711 Updated",
    "studentCount": 0
  },
  "success": true,
  "message": "Faculty created successfully.",
  "statusCode": 201,
  "timestamp": "2026-11-03T19:55:00.579292Z"
}
```

---

### Test Case: 33. Create Faculty Code Conflict (Duplicate Code)

**Request:**
```http
POST /api/v1/universities/7/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "name": "Faculty of Medicine Duplicate",
  "code": "MED_ALEX"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  49,
  48,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  67,
  111,
  110,
  102,
  108,
  105,
  99,
  116,
  32,
  68,
  101,
  116,
  101,
  99,
  116,
  101,
  100,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  57,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  65,
  32,
  102,
  97,
  99,
  117,
  108,
  116,
  121,
  32,
  119,
  105,
  116,
  104,
  32,
  99,
  111,
  100,
  101,
  32,
  39,
  77,
  69,
  68,
  95,
  65,
  76,
  69,
  88,
  39,
  32,
  97,
  108,
  114,
  101,
  97,
  100,
  121,
  32,
  101,
  120,
  105,
  115,
  116,
  115,
  32,
  105,
  110,
  32,
  65,
  108,
  101,
  120,
  97,
  110,
  100,
  114,
  105,
  97,
  32,
  85,
  110,
  105,
  118,
  101,
  114,
  115,
  105,
  116,
  121,
  32,
  54,
  52,
  52,
  54,
  55,
  49,
  49,
  32,
  85,
  112,
  100,
  97,
  116,
  101,
  100,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  117,
  110,
  105,
  118,
  101,
  114,
  115,
  105,
  116,
  105,
  101,
  115,
  47,
  55,
  47,
  102,
  97,
  99,
  117,
  108,
  116,
  105,
  101,
  115,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  55,
  49,
  97,
  57,
  50,
  52,
  98,
  97,
  57,
  49,
  51,
  57,
  52,
  101,
  55,
  102,
  51,
  56,
  53,
  102,
  98,
  98,
  52,
  56,
  101,
  55,
  57,
  97,
  52,
  100,
  102,
  54,
  45,
  57,
  99,
  54,
  99,
  102,
  98,
  54,
  56,
  100,
  53,
  54,
  99,
  49,
  97,
  48,
  49,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 34. Update Faculty Information (PUT /faculties/{id})

**Request:**
```http
PUT /api/v1/faculties/8 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "name": "Faculty of Medicine & Health",
  "code": "MED_HEALTH"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "facultyId": 8,
    "name": "Faculty of Medicine \u0026 Health",
    "code": "MED_HEALTH",
    "universityId": 7,
    "universityName": "Alexandria University 6446711 Updated",
    "studentCount": 0
  },
  "success": true,
  "message": "Faculty updated successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.6931372Z"
}
```

---

### Test Case: 35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})

**Request:**
```http
DELETE /api/v1/faculties/8 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 36. Soft-Delete University as Admin (DELETE /universities/{id})

**Request:**
```http
DELETE /api/v1/universities/7 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 37. Search Student Directory (GET /students?search=...)

**Request:**
```http
GET /api/v1/students?search=Ahmed&page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "items": [
      {
        "studentId": 11,
        "firstName": "Ahmed",
        "lastName": "Tyson",
        "email": "live_student_1333680023@teamup.edu",
        "academicYear": 3,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": []
      },
      {
        "studentId": 12,
        "firstName": "Ahmed",
        "lastName": "Tyson",
        "email": "live_student_1933805888@teamup.edu",
        "academicYear": 3,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": []
      },
      {
        "studentId": 2,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_148184769@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      },
      {
        "studentId": 3,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_17361202@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      },
      {
        "studentId": 4,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_662814197@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      },
      {
        "studentId": 5,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_664792274@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      },
      {
        "studentId": 6,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_1887857836@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      },
      {
        "studentId": 7,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_849246869@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      },
      {
        "studentId": 14,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_267034051@teamup.edu",
        "academicYear": 4,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "interests": [
          "Artificial Intelligence",
          "Cloud \u0026 Distributed Systems"
        ]
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 9,
    "totalPages": 1
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.8892704Z"
}
```

---

### Test Case: 38. Get Public Student Profile Card (GET /students/{id})

**Request:**
```http
GET /api/v1/students/14 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJkN2UxNzViMS00NzcyLTQ5NDEtOWZkOS00YzQ3N2MzNjk1YzIiLCJleHAiOjE3OTM3MzkyOTcsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.DCbCV5EdlO6-AT_XNbkK8qptBqyIf0dSByCsDdpVLPI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 14,
    "firstName": "Ahmed (Lead)",
    "lastName": "Tyson",
    "academicYear": 4,
    "facultyName": "Faculty of Computers and Artificial Intelligence",
    "universityName": "Cairo University",
    "profile": {
      "bio": "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
      "photoUrl": "https://avatars.githubusercontent.com/u/AhmedTyson",
      "gitHubUrl": "https://github.com/AhmedTyson",
      "linkedInUrl": "https://linkedin.com/in/ahmedtyson"
    },
    "interests": [
      "Artificial Intelligence",
      "Cloud \u0026 Distributed Systems"
    ]
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.9184356Z"
}
```

---

### Test Case: 39. Request Password Reset Token (POST /auth/forgot-password)

**Request:**
```http
POST /api/v1/auth/forgot-password HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "live_student_267034051@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "message": "If your email is registered, a password reset link has been dispatched.",
    "resetToken": "gLwGNRZkqjjLbB8H02bPeL\u002B0aBv3WKI4rirxcwYe5WmkDXl6FJi3qp9SNVxDLKtr6vypEaBZT6ALCCmLbekhhA=="
  },
  "success": true,
  "message": "Password reset request processed.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:00.981593Z"
}
```

---

### Test Case: 40. Reset Password with Token (POST /auth/reset-password)

**Request:**
```http
POST /api/v1/auth/reset-password HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "live_student_267034051@teamup.edu",
  "token": "gLwGNRZkqjjLbB8H02bPeL+0aBv3WKI4rirxcwYe5WmkDXl6FJi3qp9SNVxDLKtr6vypEaBZT6ALCCmLbekhhA==",
  "newPassword": "UpdatedSecurePassword2026!"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "Password reset successfully. You can now login with your new credentials.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.1278453Z"
}
```

---

### Test Case: 41. Authenticate with New Password (POST /auth/login)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "live_student_267034051@teamup.edu",
  "password": "UpdatedSecurePassword2026!"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCAoTGVhZCkgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMjk3NzYyYzgtZDQ1ZS00OTBmLTk3NTktNzI0ZjhkYzkyYzVlIiwiZXhwIjoxNzkzNzM5MzAxLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.XKZLw4GxGM9B6Qb62y-bUT34GiNDK7kfCS1h-Ei_O6o",
    "refreshToken": "tKwdbcqZB1mmUJ46DzY/V\u002B1InUWerrfCVQDaumurMqgipcXcYvNPSVd1DrZdbzNuBRW6uX2BMCIUpm5GyNDYSA==",
    "expiresAt": "2026-11-03T20:55:01.2612617Z",
    "student": {
      "studentId": 14,
      "firstName": "Ahmed (Lead)",
      "lastName": "Tyson",
      "email": "live_student_267034051@teamup.edu",
      "academicYear": 4,
      "role": "Student",
      "facultyId": 1,
      "facultyName": "Faculty of Computers and Artificial Intelligence",
      "universityName": "Cairo University"
    }
  },
  "success": true,
  "message": "Login successful.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.2701132Z"
}
```

---

### Test Case: 42. Admin System Overview Metrics (GET /admin/dashboard)

**Request:**
```http
GET /api/v1/admin/dashboard HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "activeStudentsCount": 10,
    "activeUniversitiesCount": 2,
    "activeFacultiesCount": 3,
    "softDeletedStudentsCount": 4,
    "softDeletedUniversitiesCount": 5,
    "softDeletedFacultiesCount": 5,
    "roleDistribution": {
      "PlatformAdmin": 1,
      "Student": 9
    }
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.4276Z"
}
```

---

### Test Case: 43. Promote Student to OrgAdmin (PATCH /admin/students/{id}/role)

**Request:**
```http
PATCH /api/v1/admin/students/14/role HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
Content-Type: application/json

{
  "role": "OrgAdmin"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 14,
    "firstName": "Ahmed (Lead)",
    "lastName": "Tyson",
    "email": "live_student_267034051@teamup.edu",
    "academicYear": 4,
    "role": "OrgAdmin",
    "facultyId": 1,
    "facultyName": "Faculty of Computers and Artificial Intelligence",
    "universityName": "Cairo University"
  },
  "success": true,
  "message": "Student role updated successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.5508877Z"
}
```

---

### Test Case: 44. Browse Soft-Deleted Audit Log (GET /admin/soft-deleted)

**Request:**
```http
GET /api/v1/admin/soft-deleted?entityType=university HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [
    {
      "id": 7,
      "entityType": "University",
      "displayName": "Alexandria University 6446711 Updated",
      "deletedAt": "2026-11-03T19:55:00.7599481"
    },
    {
      "id": 5,
      "entityType": "University",
      "displayName": "Alexandria University 583737299 Updated",
      "deletedAt": "2026-11-03T19:44:14.6272555"
    },
    {
      "id": 4,
      "entityType": "University",
      "displayName": "Alexandria University 983230740 Updated",
      "deletedAt": "2026-11-03T19:33:50.9299497"
    },
    {
      "id": 3,
      "entityType": "University",
      "displayName": "Alexandria University 910023581 Updated",
      "deletedAt": "2026-11-03T19:11:50.884878"
    },
    {
      "id": 2,
      "entityType": "University",
      "displayName": "Alexandria University 1128166698 Updated",
      "deletedAt": "2026-11-03T16:39:04.3677159"
    }
  ],
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.6000401Z"
}
```

---

### Test Case: 45. Restore Soft-Deleted University (POST /admin/restore/university/{id})

**Request:**
```http
POST /api/v1/admin/restore/university/7 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "university with ID 7 has been restored successfully.",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.7038006Z"
}
```

---

### Test Case: 46. Soft-Delete Student Account (DELETE /students/me)

**Request:**
```http
DELETE /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTQiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8yNjcwMzQwNTFAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBaG1lZCAoTGVhZCkgVHlzb24iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdHVkZW50IiwianRpIjoiMjk3NzYyYzgtZDQ1ZS00OTBmLTk3NTktNzI0ZjhkYzkyYzVlIiwiZXhwIjoxNzkzNzM5MzAxLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.XKZLw4GxGM9B6Qb62y-bUT34GiNDK7kfCS1h-Ei_O6o
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 47. Login Rejection for Soft-Deleted Student

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "email": "live_student_267034051@teamup.edu",
  "password": "UpdatedSecurePassword2026!"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  50,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  85,
  110,
  97,
  117,
  116,
  104,
  111,
  114,
  105,
  122,
  101,
  100,
  32,
  65,
  99,
  99,
  101,
  115,
  115,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  49,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  73,
  110,
  118,
  97,
  108,
  105,
  100,
  32,
  101,
  109,
  97,
  105,
  108,
  32,
  111,
  114,
  32,
  112,
  97,
  115,
  115,
  119,
  111,
  114,
  100,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  108,
  111,
  103,
  105,
  110,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  48,
  56,
  101,
  55,
  97,
  100,
  100,
  53,
  57,
  53,
  101,
  55,
  99,
  100,
  102,
  56,
  52,
  100,
  99,
  51,
  52,
  48,
  102,
  56,
  57,
  53,
  56,
  51,
  101,
  54,
  100,
  50,
  45,
  56,
  50,
  57,
  51,
  48,
  50,
  55,
  101,
  97,
  55,
  101,
  54,
  53,
  57,
  56,
  48,
  45,
  48,
  48,
  34,
  125
]
```

---

### Test Case: 48. Verify Soft-Deleted Student Omitted from Directory

**Request:**
```http
GET /api/v1/students?search=live_student_267034051@teamup.edu HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "timestamp": "2026-11-03T19:55:01.7915424Z"
}
```

---

### Test Case: 49. Invalidate Admin Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiZDI1NmJmMTgtZTBmNS00YTFjLWIxZDktYmUwNGViNGEwNzRhIiwiZXhwIjoxNzkzNzM5Mjk5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.d8uKmSFRgNSFJNukTp1IQtCy1bP5m9aTzzVJIswBGLY
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 50. Refresh Token Rejection After Logout (Token Revoked)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
  "refreshToken": "fcQ55onUXpqwyFy9iHHJDJD/oq2CywxzYSjF3rPR5mR4/lYJm9lHnE0q2S3QR63S5KeWiFpR3VBXUxbmAsQQ/Q=="
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
[
  123,
  34,
  116,
  121,
  112,
  101,
  34,
  58,
  34,
  104,
  116,
  116,
  112,
  115,
  58,
  47,
  47,
  116,
  111,
  111,
  108,
  115,
  46,
  105,
  101,
  116,
  102,
  46,
  111,
  114,
  103,
  47,
  104,
  116,
  109,
  108,
  47,
  114,
  102,
  99,
  57,
  49,
  49,
  48,
  35,
  115,
  101,
  99,
  116,
  105,
  111,
  110,
  45,
  49,
  53,
  46,
  53,
  46,
  50,
  34,
  44,
  34,
  116,
  105,
  116,
  108,
  101,
  34,
  58,
  34,
  85,
  110,
  97,
  117,
  116,
  104,
  111,
  114,
  105,
  122,
  101,
  100,
  32,
  65,
  99,
  99,
  101,
  115,
  115,
  34,
  44,
  34,
  115,
  116,
  97,
  116,
  117,
  115,
  34,
  58,
  52,
  48,
  49,
  44,
  34,
  100,
  101,
  116,
  97,
  105,
  108,
  34,
  58,
  34,
  73,
  110,
  118,
  97,
  108,
  105,
  100,
  32,
  111,
  114,
  32,
  101,
  120,
  112,
  105,
  114,
  101,
  100,
  32,
  114,
  101,
  102,
  114,
  101,
  115,
  104,
  32,
  116,
  111,
  107,
  101,
  110,
  46,
  34,
  44,
  34,
  105,
  110,
  115,
  116,
  97,
  110,
  99,
  101,
  34,
  58,
  34,
  47,
  97,
  112,
  105,
  47,
  118,
  49,
  47,
  97,
  117,
  116,
  104,
  47,
  114,
  101,
  102,
  114,
  101,
  115,
  104,
  34,
  44,
  34,
  116,
  114,
  97,
  99,
  101,
  73,
  100,
  34,
  58,
  34,
  48,
  48,
  45,
  49,
  101,
  54,
  98,
  56,
  100,
  52,
  57,
  53,
  100,
  102,
  102,
  55,
  101,
  49,
  99,
  100,
  49,
  102,
  99,
  97,
  54,
  56,
  101,
  51,
  99,
  49,
  51,
  57,
  51,
  54,
  55,
  45,
  55,
  99,
  54,
  50,
  98,
  57,
  55,
  99,
  99,
  48,
  99,
  49,
  51,
  57,
  51,
  49,
  45,
  48,
  48,
  34,
  125
]
```

---

