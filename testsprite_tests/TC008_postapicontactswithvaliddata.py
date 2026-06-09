import requests

BASE_URL = "http://localhost:10000"
TIMEOUT = 30

def test_post_api_contacts_with_valid_data():
    login_url = f"{BASE_URL}/api/auth/login"
    contacts_url = f"{BASE_URL}/api/contacts"

    # Admin credentials for authentication
    auth_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }

    headers = {"Content-Type": "application/json"}

    try:
        # Authenticate and get JWT token
        login_resp = requests.post(login_url, json=auth_payload, headers=headers, timeout=TIMEOUT)
        assert login_resp.status_code == 200, f"Login failed with status {login_resp.status_code}"
        token = login_resp.json().get("token")
        assert token is not None and isinstance(token, str) and token != "", "JWT token missing in login response"

        auth_headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json"
        }

        # Valid contact payload: supplier type example
        contact_payload = {
            "name": "Test Supplier Contact",
            "type": "Supplier",
            "phone": "+1234567890",
            "notes": "Test notes for supplier contact"
        }

        # Create a new contact
        create_resp = requests.post(contacts_url, json=contact_payload, headers=auth_headers, timeout=TIMEOUT)
        assert create_resp.status_code == 200, f"Expected status 200, got {create_resp.status_code}"
        create_resp_json = create_resp.json()
        contact_id = create_resp_json.get("id")
        assert contact_id is not None, "Created contact ID not returned in response"

    finally:
        # Clean up by deleting the created contact if it was created
        if 'contact_id' in locals():
            delete_resp = requests.delete(f"{contacts_url}/{contact_id}", headers=auth_headers, timeout=TIMEOUT)
            # According to instructions, status code can be 200 or 404 on delete
            assert delete_resp.status_code in (200, 404), f"Delete contact returned unexpected status {delete_resp.status_code}"

test_post_api_contacts_with_valid_data()