# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 21:44:05 UTC
> **Target Environment:** ASP.NET Core 10 Web API (http://localhost:5199)
> **Database Engine:** Microsoft SQL Server 2022 (Docker container 	eamup-sql:14333)
> **Architecture Standards:** RFC 7807 ProblemDetails, JWT Bearer Token, Single-Column Soft Delete (DeletedAt)

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
    "email":  "live_student_1385543752@teamup.edu",
    "firstName":  "Ahmed",
    "facultyId":  1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjAwNjRiOWQxLWJlZTgtNDA0Ny1hYmNlLWQ4MWJmODRkZTZkZSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.HzSL86AlhIhUOW7RWq4UkxQmwsWALqyf3WmazNiZuLs",
                 "refreshToken":  "3l1he5e3cAoSAt25HOpl79yqFnHX+w5scnDnekAXppvBrHZMwjSKnDHtqqgfUZjWlrhSZrGRSeY3ZGoDJVlyhg==",
                 "expiresAt":  "2026-11-03T20:44:09.0630992Z",
                 "student":  {
                                 "studentId":  10,
                                 "firstName":  "Ahmed",
                                 "lastName":  "Tyson",
                                 "email":  "live_student_1385543752@teamup.edu",
                                 "academicYear":  3,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Student registered successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:44:09.0898523Z"
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
    "email":  "live_student_1385543752@teamup.edu",
    "firstName":  "Duplicate",
    "facultyId":  1
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A student with this email address already exists.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-24db444a8d8ee3030b72fac484bab592-18799caa09bff01b-00"
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
    "type":  "https://tools.ietf.org/html/rfc9110",
    "title":  "Validation Failed",
    "status":  400,
    "detail":  "One or more validation errors occurred.",
    "instance":  "/api/v1/auth/register",
    "errors":  [
                   {
                       "field":  "password",
                       "message":  "Password is required."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must be at least 8 characters long."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must contain at least one uppercase letter."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must contain at least one lowercase letter."
                   },
                   {
                       "field":  "password",
                       "message":  "Password must contain at least one digit."
                   }
               ],
    "timestamp":  "2026-11-03T19:44:09.3676681Z"
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
    "email":  "bad_faculty_1643605571@teamup.edu",
    "firstName":  "Ghost",
    "facultyId":  99999
}
```

**Response:** `404 Not Found` — **🟢 PASSED** (Expected: `404`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title":  "Resource Not Found",
    "status":  404,
    "detail":  "Faculty with ID 99999 was not found.",
    "instance":  "/api/v1/auth/register",
    "traceId":  "00-599650d45ad24bd9f8169748ca859efc-e21f30a7208967f1-00"
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
    "email":  "live_student_1385543752@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjAzZjY3M2VmLTMxOGMtNGJlMS04YjZjLWY1YTc1YjUxNWI1OCIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.nAAMsKBp5Gpe5zcvEka49DHteB1Ab4F1uWGObEOc3mA",
                 "refreshToken":  "H34Zg4qAxrTDjXwhjjxA2TvxpQLKnw+poiLXvZw+zVxHjG7dJvWNl566R9K1d3ChPgFScXxGLZgfcJYQgeWW/Q==",
                 "expiresAt":  "2026-11-03T20:44:09.6629019Z",
                 "student":  {
                                 "studentId":  10,
                                 "firstName":  "Ahmed",
                                 "lastName":  "Tyson",
                                 "email":  "live_student_1385543752@teamup.edu",
                                 "academicYear":  3,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Login successful.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:09.7168684Z"
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
    "email":  "live_student_1385543752@teamup.edu"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-db2d48471edf997a4245f1b0abb72078-5d03c5a1b96e95ad-00"
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
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-1a166e49e5aea54863f78f54496db60d-e09befda99c9a2a6-00"
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
    "refreshToken":  "H34Zg4qAxrTDjXwhjjxA2TvxpQLKnw+poiLXvZw+zVxHjG7dJvWNl566R9K1d3ChPgFScXxGLZgfcJYQgeWW/Q=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE",
                 "refreshToken":  "rA5q/GJG1U6eb/7gket49CtJvnwmDgM2IFBreUXv7KjRYqJAcDYmPM66mEs6RqjZJvTFzdY6uh1GzryQir6siQ==",
                 "expiresAt":  "2026-11-03T20:44:09.9947968Z",
                 "student":  {
                                 "studentId":  10,
                                 "firstName":  "Ahmed",
                                 "lastName":  "Tyson",
                                 "email":  "live_student_1385543752@teamup.edu",
                                 "academicYear":  3,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Token refreshed successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.0116911Z"
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
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid or expired refresh token.",
    "instance":  "/api/v1/auth/refresh",
    "traceId":  "00-f0d41e2bc665f6d2a13fd725e81693d4-6345f87748f8dab1-00"
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
    "traceId":  "00-2db3744f06b8d56e07d388f96f7d2dce-fb7ff5c094c7634e-00"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  10,
                 "firstName":  "Ahmed",
                 "lastName":  "Tyson",
                 "email":  "live_student_1385543752@teamup.edu",
                 "academicYear":  3,
                 "role":  "Student",
                 "facultyId":  1,
                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                 "universityName":  "Cairo University",
                 "profile":  null,
                 "interests":  [

                               ]
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.3242919Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
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
    "data":  {
                 "studentId":  10,
                 "firstName":  "Ahmed (Lead)",
                 "lastName":  "Tyson",
                 "email":  "live_student_1385543752@teamup.edu",
                 "academicYear":  4,
                 "role":  "Student",
                 "facultyId":  1,
                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                 "universityName":  "Cairo University",
                 "profile":  null,
                 "interests":  [

                               ]
             },
    "success":  true,
    "message":  "Student account updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.4518358Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/10/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "bio":  null,
                 "photoUrl":  null,
                 "gitHubUrl":  null,
                 "linkedInUrl":  null
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.5531973Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
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
    "data":  {
                 "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
                 "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
                 "gitHubUrl":  "https://github.com/AhmedTyson",
                 "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
             },
    "success":  true,
    "message":  "Profile updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.6953373Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/10/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
                 "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
                 "gitHubUrl":  "https://github.com/AhmedTyson",
                 "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.7481527Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Interest added successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.8596416Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
Content-Type: application/json

{
    "interest":  "Cloud \u0026 Distributed Systems"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Interest added successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:10.9211804Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/10/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 "Artificial Intelligence",
                 "Cloud \u0026 Distributed Systems"
             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:11.0022668Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/10/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 "Cloud \u0026 Distributed Systems"
             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:11.1293807Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
Content-Type: application/json

{
    "interest":  "Artificial Intelligence"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Interest added successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:11.192939Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/10/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [
                 "Artificial Intelligence",
                 "Cloud \u0026 Distributed Systems"
             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:11.2432907Z"
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
    "password":  "Adm!0RK8zvFIO2BDTX",
    "email":  "admin@teamup.local"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY",
                 "refreshToken":  "Or6aq2pPU1vVxhS+hQ4R58U141S4wIx15AHF7L6S7Zvn+1oKxwCaDfWANDnYqL923c6VKqrbM6V8Pt5BLlnNOg==",
                 "expiresAt":  "2026-11-03T20:44:13.448696Z",
                 "student":  {
                                 "studentId":  1,
                                 "firstName":  "Platform",
                                 "lastName":  "Admin",
                                 "email":  "admin@teamup.local",
                                 "academicYear":  1,
                                 "role":  "PlatformAdmin",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Login successful.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:13.467017Z"
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
    "data":  [
                 {
                     "universityId":  1,
                     "name":  "Cairo University",
                     "city":  "Giza",
                     "country":  "Egypt",
                     "website":  "https://cu.edu.eg",
                     "faculties":  [
                                       {
                                           "facultyId":  1,
                                           "name":  "Faculty of Computers and Artificial Intelligence",
                                           "code":  "FCAI"
                                       },
                                       {
                                           "facultyId":  2,
                                           "name":  "Faculty of Engineering",
                                           "code":  "ENG"
                                       }
                                   ]
                 }
             ],
    "success":  true,
    "message":  "Academic catalog retrieved successfully for registration lookups.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:13.5835706Z"
}
```

---

### Test Case: 25. List Universities with Paging (GET /universities)

**Request:**
```http
GET /api/v1/universities?page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "items":  [
                               {
                                   "universityId":  1,
                                   "name":  "Cairo University",
                                   "city":  "Giza",
                                   "country":  "Egypt",
                                   "website":  "https://cu.edu.eg",
                                   "facultyCount":  2
                               }
                           ],
                 "page":  1,
                 "pageSize":  10,
                 "totalCount":  1,
                 "totalPages":  1
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:13.7326178Z"
}
```

---

### Test Case: 26. Get University Details with Faculties (GET /universities/{id})

**Request:**
```http
GET /api/v1/universities/1 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "universityId":  1,
                 "name":  "Cairo University",
                 "city":  "Giza",
                 "country":  "Egypt",
                 "website":  "https://cu.edu.eg",
                 "faculties":  [
                                   {
                                       "facultyId":  1,
                                       "name":  "Faculty of Computers and Artificial Intelligence",
                                       "code":  "FCAI"
                                   },
                                   {
                                       "facultyId":  2,
                                       "name":  "Faculty of Engineering",
                                       "code":  "ENG"
                                   }
                               ]
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:13.8384318Z"
}
```

---

### Test Case: 27. Create University as Student Forbidden (Role Guard Check)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Student Unauthorized University",
    "city":  "Cairo"
}
```

**Response:** `403 Forbidden` — **🟢 PASSED** (Expected: `403`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.4",
    "title":  "Forbidden",
    "status":  403,
    "traceId":  "00-6bfaedcbd1c25ea82f2936585f9412c5-d5669a630b8befde-00"
}
```

---

### Test Case: 28. Create Accredited University as Admin (POST /universities)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 583737299",
    "website":  "https://alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "universityId":  5,
                 "name":  "Alexandria University 583737299",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:44:13.9705901Z"
}
```

---

### Test Case: 29. Create University Conflict (Duplicate Name)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 583737299",
    "city":  "Alexandria"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A university named \u0027Alexandria University 583737299\u0027 already exists.",
    "instance":  "/api/v1/universities",
    "traceId":  "00-90abfcb10515a2be3012df58f14cdec8-cbed35952fec66b8-00"
}
```

---

### Test Case: 30. Update University Information (PUT /universities/{id})

**Request:**
```http
PUT /api/v1/universities/5 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
Content-Type: application/json

{
    "country":  "Egypt",
    "name":  "Alexandria University 583737299 Updated",
    "website":  "https://updated.alexu.edu.eg",
    "city":  "Alexandria"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "universityId":  5,
                 "name":  "Alexandria University 583737299 Updated",
                 "city":  "Alexandria",
                 "country":  "Egypt",
                 "website":  "https://updated.alexu.edu.eg",
                 "faculties":  [

                               ]
             },
    "success":  true,
    "message":  "University updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:14.1373483Z"
}
```

---

### Test Case: 31. List Faculties in University (GET /universities/{id}/faculties)

**Request:**
```http
GET /api/v1/universities/5/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  [

             ],
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:14.244499Z"
}
```

---

### Test Case: 32. Add Faculty to University as Admin (POST /universities/{id}/faculties)

**Request:**
```http
POST /api/v1/universities/5/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
Content-Type: application/json

{
    "code":  "MED_ALEX",
    "name":  "Faculty of Medicine"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
    "data":  {
                 "facultyId":  6,
                 "name":  "Faculty of Medicine",
                 "code":  "MED_ALEX",
                 "universityId":  5,
                 "universityName":  "Alexandria University 583737299 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty created successfully.",
    "statusCode":  201,
    "timestamp":  "2026-11-03T19:44:14.3622995Z"
}
```

---

### Test Case: 33. Create Faculty Code Conflict (Duplicate Code)

**Request:**
```http
POST /api/v1/universities/5/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
Content-Type: application/json

{
    "code":  "MED_ALEX",
    "name":  "Faculty of Medicine Duplicate"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.10",
    "title":  "Conflict Detected",
    "status":  409,
    "detail":  "A faculty with code \u0027MED_ALEX\u0027 already exists in Alexandria University 583737299 Updated.",
    "instance":  "/api/v1/universities/5/faculties",
    "traceId":  "00-29e35e2b0097976b38661bcd404313df-dad62e4d56f03bab-00"
}
```

---

### Test Case: 34. Update Faculty Information (PUT /faculties/{id})

**Request:**
```http
PUT /api/v1/faculties/6 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
Content-Type: application/json

{
    "code":  "MED_HEALTH",
    "name":  "Faculty of Medicine \u0026 Health"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "facultyId":  6,
                 "name":  "Faculty of Medicine \u0026 Health",
                 "code":  "MED_HEALTH",
                 "universityId":  5,
                 "universityName":  "Alexandria University 583737299 Updated",
                 "studentCount":  0
             },
    "success":  true,
    "message":  "Faculty updated successfully.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:14.5045295Z"
}
```

---

### Test Case: 35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})

**Request:**
```http
DELETE /api/v1/faculties/6 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 36. Soft-Delete University as Admin (DELETE /universities/{id})

**Request:**
```http
DELETE /api/v1/universities/5 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 37. Search Student Directory (GET /students?search=...)

**Request:**
```http
GET /api/v1/students?search=Ahmed&page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "items":  [
                               {
                                   "studentId":  2,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_148184769@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  3,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_17361202@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  4,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_662814197@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  5,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_664792274@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  6,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_1887857836@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  7,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_849246869@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               },
                               {
                                   "studentId":  10,
                                   "firstName":  "Ahmed (Lead)",
                                   "lastName":  "Tyson",
                                   "email":  "live_student_1385543752@teamup.edu",
                                   "academicYear":  4,
                                   "role":  "Student",
                                   "facultyId":  1,
                                   "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                   "universityName":  "Cairo University",
                                   "interests":  [
                                                     "Artificial Intelligence",
                                                     "Cloud \u0026 Distributed Systems"
                                                 ]
                               }
                           ],
                 "page":  1,
                 "pageSize":  10,
                 "totalCount":  7,
                 "totalPages":  1
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:14.774196Z"
}
```

---

### Test Case: 38. Get Public Student Profile Card (GET /students/{id})

**Request:**
```http
GET /api/v1/students/10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6ImI3ZjY0ZjRkLWM1NTgtNDk3Ny1hOTJkLThjZjNiMGNlZTdiNSIsImV4cCI6MTc5MzczODY0OSwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.bSL__02WXbaxblzBzeQVJ03kIn6r-d0dKtoJb6LLpUE
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "studentId":  10,
                 "firstName":  "Ahmed (Lead)",
                 "lastName":  "Tyson",
                 "academicYear":  4,
                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                 "universityName":  "Cairo University",
                 "profile":  {
                                 "bio":  "Full-stack engineer building TeamUp. Spec-Driven \u0026 Test-Driven Development advocate.",
                                 "photoUrl":  "https://avatars.githubusercontent.com/u/AhmedTyson",
                                 "gitHubUrl":  "https://github.com/AhmedTyson",
                                 "linkedInUrl":  "https://linkedin.com/in/ahmedtyson"
                             },
                 "interests":  [
                                   "Artificial Intelligence",
                                   "Cloud \u0026 Distributed Systems"
                               ]
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:14.845624Z"
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
    "email":  "live_student_1385543752@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "message":  "If your email is registered, a password reset link has been dispatched.",
                 "resetToken":  "5G346AnVgEQ1teVYajWuX25PAssTB0lrnn1FdLI8gonB5/FpjafzEvovXRUPiDSbUoCEKJz107Zag8uTYH1OcA=="
             },
    "success":  true,
    "message":  "Password reset request processed.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:14.9458054Z"
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
    "email":  "live_student_1385543752@teamup.edu",
    "newPassword":  "UpdatedSecurePassword2026!",
    "token":  "5G346AnVgEQ1teVYajWuX25PAssTB0lrnn1FdLI8gonB5/FpjafzEvovXRUPiDSbUoCEKJz107Zag8uTYH1OcA=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "success":  true,
    "message":  "Password reset successfully. You can now login with your new credentials.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:15.1169039Z"
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
    "password":  "UpdatedSecurePassword2026!",
    "email":  "live_student_1385543752@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIChMZWFkKSBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJiMzg4NjQ3Zi1jZTZhLTQ2N2ItOGUyOS1iYzFiODMxOGJhOWUiLCJleHAiOjE3OTM3Mzg2NTUsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.dzNH2TBHnRJ6figsV7S46cHBDHNDHJcBXI_EK75rYyE",
                 "refreshToken":  "KXSX41uF3c/SByRoB7mb4T62HFF4U7j4AjeedbdySoCj8XVcevBZjh8jDfrkIhwWxPNWuKx6lPuTgwH8CZMDdQ==",
                 "expiresAt":  "2026-11-03T20:44:15.2680388Z",
                 "student":  {
                                 "studentId":  10,
                                 "firstName":  "Ahmed (Lead)",
                                 "lastName":  "Tyson",
                                 "email":  "live_student_1385543752@teamup.edu",
                                 "academicYear":  4,
                                 "role":  "Student",
                                 "facultyId":  1,
                                 "facultyName":  "Faculty of Computers and Artificial Intelligence",
                                 "universityName":  "Cairo University"
                             }
             },
    "success":  true,
    "message":  "Login successful.",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:15.2770663Z"
}
```

---

### Test Case: 42. Soft-Delete Student Account (DELETE /students/me)

**Request:**
```http
DELETE /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTAiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xMzg1NTQzNzUyQHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTM4NTU0Mzc1MkB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIChMZWFkKSBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJiMzg4NjQ3Zi1jZTZhLTQ2N2ItOGUyOS1iYzFiODMxOGJhOWUiLCJleHAiOjE3OTM3Mzg2NTUsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.dzNH2TBHnRJ6figsV7S46cHBDHNDHJcBXI_EK75rYyE
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 43. Login Rejection for Soft-Deleted Student

**Request:**
```http
POST /api/v1/auth/login HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "password":  "UpdatedSecurePassword2026!",
    "email":  "live_student_1385543752@teamup.edu"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid email or password.",
    "instance":  "/api/v1/auth/login",
    "traceId":  "00-057d20f227625133055409f65aab4df9-4a08d5b97088a113-00"
}
```

---

### Test Case: 44. Verify Soft-Deleted Student Omitted from Directory

**Request:**
```http
GET /api/v1/students?search=live_student_1385543752@teamup.edu HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
    "data":  {
                 "items":  [

                           ],
                 "page":  1,
                 "pageSize":  20,
                 "totalCount":  0,
                 "totalPages":  0
             },
    "success":  true,
    "message":  "Success",
    "statusCode":  200,
    "timestamp":  "2026-11-03T19:44:15.4202842Z"
}
```

---

### Test Case: 45. Invalidate Admin Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiNjQwZWU5ODctYjJhOS00MDI0LWJlMzEtMDRkYWY1NTliMWM2IiwiZXhwIjoxNzkzNzM4NjUzLCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.NyfNrXoFE9ruahyoT1OMbgYwaKe8I0ybonzllgE-YsY
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 46. Refresh Token Rejection After Logout (Token Revoked)

**Request:**
```http
POST /api/v1/auth/refresh HTTP/1.1
Host: localhost:5199
Content-Type: application/json

{
    "refreshToken":  "rA5q/GJG1U6eb/7gket49CtJvnwmDgM2IFBreUXv7KjRYqJAcDYmPM66mEs6RqjZJvTFzdY6uh1GzryQir6siQ=="
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.2",
    "title":  "Unauthorized Access",
    "status":  401,
    "detail":  "Invalid or expired refresh token.",
    "instance":  "/api/v1/auth/refresh",
    "traceId":  "00-a39b0bbc72a3745625b036707ddb897a-59b7e24ea34bdf95-00"
}
```

---

