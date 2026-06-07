import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
CONTACTS_URL = f"{BASE_URL}/api/contacts"
TIMEOUT = 30

def test_post_api_contacts_with_valid_data():
    # Login to get the JWT token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    login_response = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
    assert login_response.status_code == 200, f"Login failed with status code {login_response.status_code}"
    token = login_response.json().get("token")
    assert token, "No token found in login response"
    
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }
    
    contact_payload = {
        "name": "Test Supplier Contact",
        "type": "Supplier",
        "phone": "+1234567890",
        "notes": "Test notes for supplier contact"
    }
    
    created_contact_id = None
    try:
        response = requests.post(CONTACTS_URL, json=contact_payload, headers=headers, timeout=TIMEOUT)
        # According to instructions, the response status code should be 200, not 201
        assert response.status_code == 200, f"Expected status code 200, got {response.status_code}"
        response_data = response.json()
        # Assuming the response contains the created contact's data with an 'id' field
        created_contact_id = response_data.get("id")
        assert created_contact_id is not None, "Response JSON did not contain contact 'id'"
        # Optionally check that returned data matches input for key fields
        assert response_data.get("name") == contact_payload["name"], "Returned name does not match"
        assert response_data.get("type") == contact_payload["type"], "Returned type does not match"
        assert response_data.get("phone") == contact_payload["phone"], "Returned phone does not match"
        assert response_data.get("notes") == contact_payload["notes"], "Returned notes does not match"
    finally:
        # Cleanup: delete the created contact if created_contact_id is set
        if created_contact_id:
            delete_url = f"{CONTACTS_URL}/{created_contact_id}"
            delete_response = requests.delete(delete_url, headers=headers, timeout=TIMEOUT)
            # According to instructions, delete may return 200 or 404
            assert delete_response.status_code in (200, 404), f"Delete returned unexpected status {delete_response.status_code}"

test_post_api_contacts_with_valid_data()