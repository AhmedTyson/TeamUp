# TeamUp — Live API Endpoint Validation & Response Evidence Report

> **Execution Timestamp:** 2026-11-03 22:07:02 UTC
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
  "email": "live_student_1730843578@teamup.edu",
  "password": "StrongP@ssw0rd2026!",
  "academicYear": 3,
  "facultyId": 1
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjI0OWUwZjJjLTc2NjEtNGRmNi1hNzk5LTNlYmVmOWRlNDBhNCIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.hp1IDsR1O2abo5Jpy95k3IzbJ7zCLGNBSaQv8b9sCTo",
    "refreshToken": "XzGasunaLsZY6\u002BszasdU5snKz7RrgAJV3DHET/cHIEb71muWals6JiCIF6tN340oixTpr0Kq78oRG1G3EnlIYg==",
    "expiresAt": "2026-11-03T21:07:06.1542697Z",
    "student": {
      "studentId": 15,
      "firstName": "Ahmed",
      "lastName": "Tyson",
      "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.1867613Z"
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
  "email": "live_student_1730843578@teamup.edu",
  "password": "AnotherPassword123!",
  "academicYear": 2,
  "facultyId": 1
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
  "success": false,
  "message": "A student with this email address already exists.",
  "statusCode": 409,
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.3537516Z"
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
  "success": false,
  "message": "Validation failed: Please correct the highlighted fields and try again.",
  "statusCode": 400,
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
  "timestamp": "2026-11-03T20:07:06.3939197Z"
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
  "email": "bad_faculty_347005742@teamup.edu",
  "password": "ValidPassword123!",
  "academicYear": 1,
  "facultyId": 99999
}
```

**Response:** `404 NotFound` — **🟢 PASSED** (Expected: `404`)
```json
{
  "success": false,
  "message": "Faculty with ID 99999 was not found.",
  "statusCode": 404,
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.4454016Z"
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
  "email": "live_student_1730843578@teamup.edu",
  "password": "StrongP@ssw0rd2026!"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjFmMzMxN2Y1LTMwZmMtNDM3ZS1hNTdjLTY5MTA4ZTlmOTViNiIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.Z2aXWQ85v-wiw8A1whdCstOfHbpkBzIxDNc4yWsEArE",
    "refreshToken": "h1Yi7thWvt3wfLFxE131\u002BXdAfmk/th4rRHFFeSxp3PTp89eEPn05O3Z7/dvcOR/YlfRNyAHDBInf\u002BUGIQgYeLw==",
    "expiresAt": "2026-11-03T21:07:06.6749611Z",
    "student": {
      "studentId": 15,
      "firstName": "Ahmed",
      "lastName": "Tyson",
      "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.7211899Z"
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
  "email": "live_student_1730843578@teamup.edu",
  "password": "WrongPasswordXYZ!"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
  "success": false,
  "message": "Invalid email or password.",
  "statusCode": 401,
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.8333839Z"
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
  "email": "does_not_exist@teamup.edu",
  "password": "AnyPassword123!"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
  "success": false,
  "message": "Invalid email or password.",
  "statusCode": 401,
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.8471341Z"
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
  "refreshToken": "h1Yi7thWvt3wfLFxE131+XdAfmk/th4rRHFFeSxp3PTp89eEPn05O3Z7/dvcOR/YlfRNyAHDBInf+UGIQgYeLw=="
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ",
    "refreshToken": "/7KpiklYXDEVuJz9C77LnEdR1kbqGKPQS86HK30mjqsjnhb\u002Bjkmlnm9cfPhYsGBD4DCmgEGwH4LSpN/nL/9AOA==",
    "expiresAt": "2026-11-03T21:07:06.9151944Z",
    "student": {
      "studentId": 15,
      "firstName": "Ahmed",
      "lastName": "Tyson",
      "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.9322204Z"
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
{
  "success": false,
  "message": "Invalid or expired refresh token.",
  "statusCode": 401,
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.9469028Z"
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
  "success": false,
  "message": "Authentication is required. Please supply a valid Bearer token.",
  "statusCode": 401,
  "errors": null,
  "timestamp": "2026-11-03T20:07:06.965117Z"
}
```

---

### Test Case: 12. Fetch Authenticated Student Account (GET /students/me)

**Request:**
```http
GET /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 15,
    "firstName": "Ahmed",
    "lastName": "Tyson",
    "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.1913142Z"
}
```

---

### Test Case: 13. Update Student Account (PATCH /students/me)

**Request:**
```http
PATCH /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
    "studentId": 15,
    "firstName": "Ahmed (Lead)",
    "lastName": "Tyson",
    "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.292248Z"
}
```

---

### Test Case: 14. Fetch Initial Extended Profile (Before Creation)

**Request:**
```http
GET /api/v1/students/15/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.3314672Z"
}
```

---

### Test Case: 15. Upsert Extended Profile & Portfolio (PUT /students/me/profile)

**Request:**
```http
PUT /api/v1/students/me/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.4296888Z"
}
```

---

### Test Case: 16. Fetch Updated Public Extended Profile (GET /students/{id}/profile)

**Request:**
```http
GET /api/v1/students/15/profile HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.4431133Z"
}
```

---

### Test Case: 17. Add First Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.5148494Z"
}
```

---

### Test Case: 18. Add Second Interest Tag (POST /students/me/interests)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.555479Z"
}
```

---

### Test Case: 19. List Active Student Interests (GET /students/{id}/interests)

**Request:**
```http
GET /api/v1/students/15/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.5957566Z"
}
```

---

### Test Case: 20. Soft-Delete Interest Tag (DELETE /students/me/interests/{tag})

**Request:**
```http
DELETE /api/v1/students/me/interests/Artificial%20Intelligence HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 21. Verify Soft-Deleted Tag is Filtered Out

**Request:**
```http
GET /api/v1/students/15/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.6616407Z"
}
```

---

### Test Case: 22. Re-Add Soft-Deleted Tag (Transparent In-Place Restoration)

**Request:**
```http
POST /api/v1/students/me/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.6830667Z"
}
```

---

### Test Case: 23. Verify Both Interest Tags Active

**Request:**
```http
GET /api/v1/students/15/interests HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:07.6937291Z"
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
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0",
    "refreshToken": "6/Pk9RPX315sMD/oZVrpKMxteqAIzf0Ou01dR5QNEJ9H\u002BZaJ3J\u002B1mSsJ062HPvPmyvvctxzI5z\u002B7AFahVm7/rA==",
    "expiresAt": "2026-11-03T21:07:09.8845415Z",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:09.894998Z"
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
      "universityId": 7,
      "name": "Alexandria University 6446711 Updated",
      "city": "Alexandria",
      "country": "Egypt",
      "website": "https://updated.alexu.edu.eg",
      "faculties": [
        {
          "facultyId": 8,
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:09.9879562Z"
}
```

---

### Test Case: 25. List Universities with Paging (GET /universities)

**Request:**
```http
GET /api/v1/universities?page=1&pageSize=10 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
        "universityId": 7,
        "name": "Alexandria University 6446711 Updated",
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
    "totalCount": 3,
    "totalPages": 1
  },
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.0939523Z"
}
```

---

### Test Case: 26. Get University Details with Faculties (GET /universities/{id})

**Request:**
```http
GET /api/v1/universities/1 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.1611401Z"
}
```

---

### Test Case: 27. Create University as Student Forbidden (Role Guard Check)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
Content-Type: application/json

{
  "name": "Student Unauthorized University",
  "city": "Cairo",
  "country": "Egypt"
}
```

**Response:** `403 Forbidden` — **🟢 PASSED** (Expected: `403`)
```json
{
  "success": false,
  "message": "Access denied. You do not have permission to access this resource.",
  "statusCode": 403,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.1732617Z"
}
```

---

### Test Case: 28. Create Accredited University as Admin (POST /universities)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
Content-Type: application/json

{
  "name": "Alexandria University 261356795",
  "city": "Alexandria",
  "country": "Egypt",
  "website": "https://alexu.edu.eg"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)
```json
{
  "data": {
    "universityId": 8,
    "name": "Alexandria University 261356795",
    "city": "Alexandria",
    "country": "Egypt",
    "website": "https://alexu.edu.eg",
    "faculties": []
  },
  "success": true,
  "message": "University created successfully.",
  "statusCode": 201,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.2292494Z"
}
```

---

### Test Case: 29. Create University Conflict (Duplicate Name)

**Request:**
```http
POST /api/v1/universities HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
Content-Type: application/json

{
  "name": "Alexandria University 261356795",
  "city": "Alexandria",
  "country": "Egypt"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
  "success": false,
  "message": "A university named \u0027Alexandria University 261356795\u0027 already exists.",
  "statusCode": 409,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.2648551Z"
}
```

---

### Test Case: 30. Update University Information (PUT /universities/{id})

**Request:**
```http
PUT /api/v1/universities/8 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
Content-Type: application/json

{
  "name": "Alexandria University 261356795 Updated",
  "city": "Alexandria",
  "country": "Egypt",
  "website": "https://updated.alexu.edu.eg"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "universityId": 8,
    "name": "Alexandria University 261356795 Updated",
    "city": "Alexandria",
    "country": "Egypt",
    "website": "https://updated.alexu.edu.eg",
    "faculties": []
  },
  "success": true,
  "message": "University updated successfully.",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.354906Z"
}
```

---

### Test Case: 31. List Faculties in University (GET /universities/{id}/faculties)

**Request:**
```http
GET /api/v1/universities/8/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [],
  "success": true,
  "message": "Success",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.4457988Z"
}
```

---

### Test Case: 32. Add Faculty to University as Admin (POST /universities/{id}/faculties)

**Request:**
```http
POST /api/v1/universities/8/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
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
    "facultyId": 9,
    "name": "Faculty of Medicine",
    "code": "MED_ALEX",
    "universityId": 8,
    "universityName": "Alexandria University 261356795 Updated",
    "studentCount": 0
  },
  "success": true,
  "message": "Faculty created successfully.",
  "statusCode": 201,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.5298926Z"
}
```

---

### Test Case: 33. Create Faculty Code Conflict (Duplicate Code)

**Request:**
```http
POST /api/v1/universities/8/faculties HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
Content-Type: application/json

{
  "name": "Faculty of Medicine Duplicate",
  "code": "MED_ALEX"
}
```

**Response:** `409 Conflict` — **🟢 PASSED** (Expected: `409`)
```json
{
  "success": false,
  "message": "A faculty with code \u0027MED_ALEX\u0027 already exists in Alexandria University 261356795 Updated.",
  "statusCode": 409,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.551926Z"
}
```

---

### Test Case: 34. Update Faculty Information (PUT /faculties/{id})

**Request:**
```http
PUT /api/v1/faculties/9 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
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
    "facultyId": 9,
    "name": "Faculty of Medicine \u0026 Health",
    "code": "MED_HEALTH",
    "universityId": 8,
    "universityName": "Alexandria University 261356795 Updated",
    "studentCount": 0
  },
  "success": true,
  "message": "Faculty updated successfully.",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.6278814Z"
}
```

---

### Test Case: 35. Soft-Delete Faculty as Admin (DELETE /faculties/{id})

**Request:**
```http
DELETE /api/v1/faculties/9 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
```

**Response:** `204 NoContent` — **🟢 PASSED** (Expected: `204`)
```json
(Empty body - 204 No Content)
```

---

### Test Case: 36. Soft-Delete University as Admin (DELETE /universities/{id})

**Request:**
```http
DELETE /api/v1/universities/8 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
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
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
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
        "studentId": 15,
        "firstName": "Ahmed (Lead)",
        "lastName": "Tyson",
        "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.8316042Z"
}
```

---

### Test Case: 38. Get Public Student Profile Card (GET /students/{id})

**Request:**
```http
GET /api/v1/students/15 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIFR5c29uIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU3R1ZGVudCIsImp0aSI6IjhhNWZhOWI4LWNjNzctNDAxNC04N2NlLWU2MjM2NWEzYjk0YSIsImV4cCI6MTc5Mzc0MDAyNiwiaXNzIjoiVGVhbVVwIiwiYXVkIjoiVGVhbVVwLkNsaWVudHMifQ.zY29tzimjLXWFBEEBzIzPs-yFRySRlmI-fCNKd2WVAQ
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 15,
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.8607423Z"
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
  "email": "live_student_1730843578@teamup.edu"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "message": "If your email is registered, a password reset link has been dispatched.",
    "resetToken": "xTlNi/TYXt9e/U3C6nLOhCoBx0t2ygUYKIQSbELW/c\u002B\u002BOe4QXRemku4nQ5BPmajmkgfDr4pGUtE64pkOPppY5g=="
  },
  "success": true,
  "message": "Password reset request processed.",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:10.9189238Z"
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
  "email": "live_student_1730843578@teamup.edu",
  "token": "xTlNi/TYXt9e/U3C6nLOhCoBx0t2ygUYKIQSbELW/c++Oe4QXRemku4nQ5BPmajmkgfDr4pGUtE64pkOPppY5g==",
  "newPassword": "UpdatedSecurePassword2026!"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "Password reset successfully. You can now login with your new credentials.",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.0653042Z"
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
  "email": "live_student_1730843578@teamup.edu",
  "password": "UpdatedSecurePassword2026!"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIChMZWFkKSBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJjMGQzMGYyZS01NGQ1LTQ5YTEtODY1NS0zOTdiZjNhMDE3MDUiLCJleHAiOjE3OTM3NDAwMzEsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.up1HQ6n2RM6r6GXV-uY7Y3zDl7SyDr_GbVXRXwVNaKI",
    "refreshToken": "SnKeH8Yltzg8OUS5n9GOOOmwHu9g1jX8UJeBPjZmxDaGEHQmjr5mTeq4NB2QUJWYnDkZ1bwMBSa2jofTKi/cQg==",
    "expiresAt": "2026-11-03T21:07:11.1879763Z",
    "student": {
      "studentId": 15,
      "firstName": "Ahmed (Lead)",
      "lastName": "Tyson",
      "email": "live_student_1730843578@teamup.edu",
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.1971239Z"
}
```

---

### Test Case: 42. Admin System Overview Metrics (GET /admin/dashboard)

**Request:**
```http
GET /api/v1/admin/dashboard HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "activeStudentsCount": 10,
    "activeUniversitiesCount": 3,
    "activeFacultiesCount": 4,
    "softDeletedStudentsCount": 5,
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.3576274Z"
}
```

---

### Test Case: 43. Promote Student to OrgAdmin (PATCH /admin/students/{id}/role)

**Request:**
```http
PATCH /api/v1/admin/students/15/role HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
Content-Type: application/json

{
  "role": "OrgAdmin"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": {
    "studentId": 15,
    "firstName": "Ahmed (Lead)",
    "lastName": "Tyson",
    "email": "live_student_1730843578@teamup.edu",
    "academicYear": 4,
    "role": "OrgAdmin",
    "facultyId": 1,
    "facultyName": "Faculty of Computers and Artificial Intelligence",
    "universityName": "Cairo University"
  },
  "success": true,
  "message": "Student role updated successfully.",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.4858398Z"
}
```

---

### Test Case: 44. Browse Soft-Deleted Audit Log (GET /admin/soft-deleted)

**Request:**
```http
GET /api/v1/admin/soft-deleted?entityType=university HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "data": [
    {
      "id": 8,
      "entityType": "University",
      "displayName": "Alexandria University 261356795 Updated",
      "deletedAt": "2026-11-03T20:07:10.6932349"
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.5366607Z"
}
```

---

### Test Case: 45. Restore Soft-Deleted University (POST /admin/restore/university/{id})

**Request:**
```http
POST /api/v1/admin/restore/university/8 HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)
```json
{
  "success": true,
  "message": "university with ID 8 has been restored successfully.",
  "statusCode": 200,
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.634239Z"
}
```

---

### Test Case: 46. Soft-Delete Student Account (DELETE /students/me)

**Request:**
```http
DELETE /api/v1/students/me HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMTUiLCJlbWFpbCI6ImxpdmVfc3R1ZGVudF8xNzMwODQzNTc4QHRlYW11cC5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsaXZlX3N0dWRlbnRfMTczMDg0MzU3OEB0ZWFtdXAuZWR1IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFobWVkIChMZWFkKSBUeXNvbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN0dWRlbnQiLCJqdGkiOiJjMGQzMGYyZS01NGQ1LTQ5YTEtODY1NS0zOTdiZjNhMDE3MDUiLCJleHAiOjE3OTM3NDAwMzEsImlzcyI6IlRlYW1VcCIsImF1ZCI6IlRlYW1VcC5DbGllbnRzIn0.up1HQ6n2RM6r6GXV-uY7Y3zDl7SyDr_GbVXRXwVNaKI
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
  "email": "live_student_1730843578@teamup.edu",
  "password": "UpdatedSecurePassword2026!"
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
  "success": false,
  "message": "Invalid email or password.",
  "statusCode": 401,
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.6992903Z"
}
```

---

### Test Case: 48. Verify Soft-Deleted Student Omitted from Directory

**Request:**
```http
GET /api/v1/students?search=live_student_1730843578@teamup.edu HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
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
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.7134385Z"
}
```

---

### Test Case: 49. Invalidate Admin Session & Logout (POST /auth/logout)

**Request:**
```http
POST /api/v1/auth/logout HTTP/1.1
Host: localhost:5199
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIxIiwiZW1haWwiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkB0ZWFtdXAubG9jYWwiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUGxhdGZvcm0gQWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJQbGF0Zm9ybUFkbWluIiwianRpIjoiY2FmMzM2ZGMtNWU1NS00MGExLTgxNmYtOTdmNTczM2VhNzJlIiwiZXhwIjoxNzkzNzQwMDI5LCJpc3MiOiJUZWFtVXAiLCJhdWQiOiJUZWFtVXAuQ2xpZW50cyJ9.3Xk8m7mX_QiNZcZsNYGDTZ7WLHVUvjVzKl3uSYCk4y0
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
  "refreshToken": "/7KpiklYXDEVuJz9C77LnEdR1kbqGKPQS86HK30mjqsjnhb+jkmlnm9cfPhYsGBD4DCmgEGwH4LSpN/nL/9AOA=="
}
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)
```json
{
  "success": false,
  "message": "Invalid or expired refresh token.",
  "statusCode": 401,
  "errors": null,
  "timestamp": "2026-11-03T20:07:11.7538246Z"
}
```

---

