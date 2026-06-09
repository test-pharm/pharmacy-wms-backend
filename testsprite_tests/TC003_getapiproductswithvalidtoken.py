import requests

BASE_URL = "http://localhost:10000"
LOGIN_URL = f"{BASE_URL}/api/auth/login"
PRODUCTS_URL = f"{BASE_URL}/api/products"
TIMEOUT = 30

def test_get_api_products_with_valid_token():
    # Login as admin to get valid JWT token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        login_response = requests.post(LOGIN_URL, json=login_payload, timeout=TIMEOUT)
        assert login_response.status_code == 200, f"Login failed with status code {login_response.status_code}"
        login_data = login_response.json()
        assert "token" in login_data, "Token not found in login response"
        token = login_data["token"]

        headers = {
            "Authorization": f"Bearer {token}"
        }

        # GET /api/products with valid token
        products_response = requests.get(PRODUCTS_URL, headers=headers, timeout=TIMEOUT)
        assert products_response.status_code == 200, f"Expected 200 OK but got {products_response.status_code}"
        products_data = products_response.json()
        assert isinstance(products_data, list), "Response is not a list of products"

    except (requests.RequestException, AssertionError) as e:
        raise AssertionError(f"Test failed: {e}")

test_get_api_products_with_valid_token()