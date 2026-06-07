import requests

BASE_URL = "http://localhost:10000"
TIMEOUT = 30

def test_postapiauthregisterwithadminprivileges():
    url = f"{BASE_URL}/api/auth/register/admin"
    payload = {
        "email": "newadmin@pharmacy.com",
        "password": "AdminPass123!",
        "fullName": "New Admin User",
        "role": "Admin"
    }
    # The API schema for /api/auth/register/admin doesn't specify exact fields beyond email and password,
    # but the 'auth/register/admin' endpoint expects a valid payload to create a system admin.
    # We'll assume fullName and role can be included or are ignored if unsupported.

    headers = {"Content-Type": "application/json"}

    response = requests.post(url, json=payload, headers=headers, timeout=TIMEOUT)
    assert response.status_code == 200, f"Expected status 200 but got {response.status_code}"
    # Optionally, check response content if available (e.g., success message)
    # But PRD does not specify response body, so only status code assertion is required.

test_postapiauthregisterwithadminprivileges()