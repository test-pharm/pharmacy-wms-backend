import requests

BASE_URL = "http://localhost:10000"
REGISTER_ADMIN_URL = f"{BASE_URL}/api/auth/register/admin"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
TIMEOUT = 30


def test_postapiauthregisterwithadminprivileges():
    # Prepare a unique email to avoid conflict on repeated test runs
    import uuid
    unique_email = f"testadmin_{uuid.uuid4().hex[:8]}@pharmacy.com"

    # Payload for admin registration
    registration_payload = {
        "email": unique_email,
        "password": "Admin@1234!",
        "fullName": "Test Admin User"
    }
    # According to PRD there is no explicit schema for registration payload detailed,
    # but this payload should be enough to register an admin user.

    try:
        # Make POST request to register a new system admin
        response = requests.post(
            REGISTER_ADMIN_URL,
            json=registration_payload,
            timeout=TIMEOUT
        )
        # Assert that response status code is 200 OK (not 201 or 404)
        assert response.status_code == 200, f"Expected status 200, got {response.status_code}"

        # Optionally verify response content for success indication if available
        json_response = response.json()
        assert isinstance(json_response, dict), "Response JSON should be a dictionary"
        # Could check for a message or user id if returned, but not specified in PRD

        # Additional step: Verify that the new admin user can successfully log in
        login_payload = {"email": unique_email, "password": "Admin@1234!"}
        login_response = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
        assert login_response.status_code == 200, f"Login failed for new admin, status {login_response.status_code}"
        login_json = login_response.json()
        assert "token" in login_json and isinstance(login_json["token"], str) and len(login_json["token"]) > 10, "JWT token missing or invalid"

    finally:
        # Cleanup: No explicit endpoint provided for deleting users.
        # Since no delete endpoint is defined in the PRD for users,
        # skipping user deletion. If such endpoint exists, deletion code should be here.
        pass


test_postapiauthregisterwithadminprivileges()