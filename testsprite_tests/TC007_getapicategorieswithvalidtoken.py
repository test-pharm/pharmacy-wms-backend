import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
CATEGORIES_URL = f"{BASE_URL}/api/categories"
TIMEOUT = 30

def test_get_api_categories_with_valid_token():
    # Authenticate as admin to get valid JWT token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        login_response = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
        assert login_response.status_code == 200, f"Login failed with status {login_response.status_code}"
        token = login_response.json().get("token") or login_response.json().get("accessToken")
        assert token is not None, "JWT token not found in login response"
    except requests.RequestException as e:
        assert False, f"Login request failed: {e}"

    headers = {
        "Authorization": f"Bearer {token}"
    }

    try:
        response = requests.get(CATEGORIES_URL, headers=headers, timeout=TIMEOUT)
        assert response.status_code == 200, f"Expected status 200 but got {response.status_code}"
        categories = response.json()
        assert isinstance(categories, list), "Response is not a list of categories"
    except requests.RequestException as e:
        assert False, f"GET /api/categories request failed: {e}"

test_get_api_categories_with_valid_token()