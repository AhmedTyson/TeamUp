# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 00:53:43 UTC
> **Target Environment:** ASP.NET Core 10 Web API (`http://localhost:5199`)
> **Database Engine:** Microsoft SQL Server 2022 (Docker container `teamup-sql:14333`)
> **Architecture Standards:** RFC 7807 ProblemDetails, JWT Bearer Token, Single-Column Soft Delete (`DeletedAt`)

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
    "status":  "Healthy"
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
    "password":  "StrongP@ssw0rd2026!",
    "academicYear":  3,
    "lastName":  "Tyson",
    "email":  "live_student_17361202@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImFmZWIyMTVkLTAwMDgtNGZkMC04NjRjLWVkMDhhMzZjNGQzMiIsImV4cCI6MTc5MzY2MzYyNywiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.C879lTXIOdM9NJomO14PXfWfa0H41ABsuMH2lkB0pTQ",
    "refreshToken":  "k9N5B9TIsRiNbmsaGD01QLG4VXRMNdcACIO9hDsPTlR7GsU9JeaCKhTMyDufdmZULp5g6gyhtsZj1iTWf7y7tA==",
    "expiresAt":  "2026-11-02T23:53:47.5898889Z",
    "student":  {
                    "studentId":  3,
                    "email":  "live_student_17361202@teamup.edu",
                    "firstName":  "Ahmed",
                    "lastName":  "Tyson",
                    "academicYear":  3,
                    "role":  "Student",
                    "facultyId":  1,
                    "facultyName":  "Faculty of Computers and Artificial Intelligence",
                    "universityName":  "Cairo University"
                }
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
    "password":  "AnotherPassword123!",
    "academicYear":  2,
    "lastName":  "User",
    "email":  "live_student_17361202@teamup.edu",
    "firstName":  "Duplicate",
    "facultyId":  1
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict detected.",
    "status":  409,
    "detail":  "A student with this email address already exists.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-1da77d3a3efb8da1ec511c401556ce55-080c9699dcb78143-00"
}
```

---

### Test Case: 04. Registration Validation (Empty Password)

**Request:**
```http
POST /api/v1/auth/register HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "",
    "academicYear":  1,
    "lastName":  "Password",
    "email":  "invalid_user@teamup.edu",
    "firstName":  "Empty",
    "facultyId":  1
}
```

**Response:** `400 Bad Request` — **🟢 PASSED** (Expected: `400`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "Invalid request.",
    "status":  400,
    "detail":  "Email and password are required.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-d9fe696cfe21042db2ff79f8a5ad6384-542edd514a8ff33a-00"
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
    "password":  "ValidPassword123!",
    "academicYear":  1,
    "lastName":  "Faculty",
    "email":  "bad_faculty_791556210@teamup.edu",
    "firstName":  "Ghost",
    "facultyId":  99999
}
```

**Response:** `404 Not Found` — **🟢 PASSED** (Expected: `404`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title":  "Resource not found.",
    "status":  404,
    "detail":  "Faculty with ID 99999 was not found.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-965c8d5c988aee4d5fc6671d8a15f3e7-385555885f86e313-00"
}
```

---

### Test Case: 06. Student Login (Valid Credentials)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "StrongP@ssw0rd2026!",
    "email":  "live_student_17361202@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjE0ZDI4MzlkLTI3NGItNGIwOC1hMTc1LTNkZGIxMGYwMzE5YSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.ISsWJVQqvn3CYceObY65UILcdXxrV4Cf8nckchHgsHY",
    "refreshToken":  "NZmYQW58J3xW5/fPx44A+UDJ+TAsxgiRISu19bFxHKqIe8JRKI1EOoP+kHc8x14eVI5bKP2NwKbhFkIDhKW/dw==",
    "expiresAt":  "2026-11-02T23:53:48.2354658Z",
    "student":  {
                    "studentId":  3,
                    "email":  "live_student_17361202@teamup.edu",
                    "firstName":  "Ahmed",
                    "lastName":  "Tyson",
                    "academicYear":  3,
                    "role":  "Student",
                    "facultyId":  1,
                    "facultyName":  "Faculty of Computers and Artificial Intelligence",
                    "universityName":  "Cairo University"
                }
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
    "password":  "WrongPasswordXYZ!",
    "email":  "live_student_17361202@teamup.edu"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized access.",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-436a8e37ecaceed8b017453a02894257-ba68e878cf93e7e7-00"
}
```

---

### Test Case: 08. Login Authentication Failure (Unknown Email)

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "AnyPassword123!",
    "email":  "does_not_exist@teamup.edu"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized access.",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-f20707299df80512cf13e044cea79993-a42544ff0996de14-00"
}
```

---

### Test Case: 09. Refresh Access Token (Token Rotation)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "NZmYQW58J3xW5/fPx44A+UDJ+TAsxgiRISu19bFxHKqIe8JRKI1EOoP+kHc8x14eVI5bKP2NwKbhFkIDhKW/dw=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI",
    "refreshToken":  "lvzGvwyaQ6ioaQMjcBlzFpHvU6EXPEauEGLKVsfWzpcUEYVhc9f/xVnIM9mK80QgmukmWgTa6Wijl6TbaMfPfA==",
    "expiresAt":  "2026-11-02T23:53:48.561376Z",
    "student":  {
                    "studentId":  3,
                    "email":  "live_student_17361202@teamup.edu",
                    "firstName":  "Ahmed",
                    "lastName":  "Tyson",
                    "academicYear":  3,
                    "role":  "Student",
                    "facultyId":  1,
                    "facultyName":  "Faculty of Computers and Artificial Intelligence",
                    "universityName":  "Cairo University"
                }
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
    "refreshToken":  "TAMPERED_REFRESH_TOKEN_1234567890_INVALID"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized access.",
    "status":  401,
    "detail":  "Invalid or expired refresh token.",
    "instance":  "/api/v1/auth/refresh",
    "traceId":  "00-45f6faa6d16564d592358be9200ed275-280b2587e5b0590a-00"
}
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
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized",
    "status":  401,
    "traceId":  "00-49fceda1dcd71fcfafa925ad7670c20f-fd92d636643044b8-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "studentId":  3,
    "email":  "live_student_17361202@teamup.edu",
    "firstName":  "Ahmed",
    "lastName":  "Tyson",
    "academicYear":  3,
    "role":  "Student",
    "facultyId":  1,
    "facultyName":  "Faculty of Computers and Artificial Intelligence",
    "universityName":  "Cairo University",
    "profile":  null,
    "interests":  [

                  ]
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
Content-Type: application/json

{
    "firstName":  "Ahmed (Lead)",
    "academicYear":  4,
    "lastName":  "Tyson"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "studentId":  3,
    "email":  "live_student_17361202@teamup.edu",
    "firstName":  "Ahmed (Lead)",
    "lastName":  "Tyson",
    "academicYear":  4,
    "role":  "Student",
    "facultyId":  1,
    "facultyName":  "Faculty of Computers and Artificial Intelligence",
    "universityName":  "Cairo University",
    "profile":  null,
    "interests":  [

                  ]
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/3/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "bio":  null,
    "photoUrl":  null,
    "gitHubUrl":  null,
    "linkedInUrl":  null
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
Content-Type: application/json

{
    "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl":  "https://github.com/AhmedTyson",
    "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl":  "https://github.com/AhmedTyson",
    "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/3/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
    "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
    "gitHubUrl":  "https://github.com/AhmedTyson",
    "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "message":  "Interest added successfully."
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
Content-Type: application/json

{
    "interest":  "Cloud \u0026 Distributed Systems"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "message":  "Interest added successfully."
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/3/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "value":  [
                  "Artificial Intelligence",
                  "Cloud \u0026 Distributed Systems"
              ],
    "Count":  2
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/3/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "value":  [
                  "Cloud \u0026 Distributed Systems"
              ],
    "Count":  1
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "message":  "Interest added successfully."
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/3/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "value":  [
                  "Artificial Intelligence",
                  "Cloud \u0026 Distributed Systems"
              ],
    "Count":  2
}
```

---

### Test Case: 24. Invalidate Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiZW1haWwiOiJsaXZlX3N0dWRlbnRfMTczNjEyMDJAdGVhbXVwLmVkdSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImxpdmVfc3R1ZGVudF8xNzM2MTIwMkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjllYzk5ODJiLWJhNjAtNDNkZS1iMWMzLTY3NTc4ZTZiZjUwZSIsImV4cCI6MTc5MzY2MzYyOCwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.a3VlyMVAsSEaf0tl6BBwwriUUjId3k8QH2LCunXJXKI
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 25. Refresh Token Rejection After Logout (Token Revoked)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "lvzGvwyaQ6ioaQMjcBlzFpHvU6EXPEauEGLKVsfWzpcUEYVhc9f/xVnIM9mK80QgmukmWgTa6Wijl6TbaMfPfA=="
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized access.",
    "status":  401,
    "detail":  "Invalid or expired refresh token.",
    "instance":  "/api/v1/auth/refresh",
    "traceId":  "00-020bdb9089f4eee85a181216ac0ad2d1-6cbd3ed75b7d703c-00"
}
```

---

