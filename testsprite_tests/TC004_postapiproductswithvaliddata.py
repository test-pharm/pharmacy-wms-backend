import requests

BASE_URL = "http://localhost:10000"

def test_post_api_products_with_valid_data():
    login_url = f"{BASE_URL}/api/auth/login"
    product_url = f"{BASE_URL}/api/products"

    # Login as admin to get JWT token
    login_payload = {
        "email": "admin@pharmacy.com",
        "password": "admin123"
    }
    try:
        login_response = requests.post(login_url, json=login_payload, timeout=30)
        assert login_response.status_code == 200, f"Login failed with status {login_response.status_code}"
        token = login_response.json().get("token")
        assert token, "JWT token is missing in login response"
    except Exception as e:
        raise AssertionError(f"Authentication failed: {e}")

    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    # Define valid product data (excluding 'name', 'price', or 'description')
    product_payload = {
        "materialName": "Test Material",
        "materialSku": "SKU-12345",
        "quantity": 10,
        "unit": "kg",
        "logNumber": "LOG987654321",
        "supplier": "Test Supplier Inc.",
        "categoryId": 1
    }

    created_product_id = None

    try:
        # Create the new product
        response = requests.post(product_url, headers=headers, json=product_payload, timeout=30)
        assert response.status_code == 201, f"Expected 201 Created but got {response.status_code}"
        resp_json = response.json()
        created_product_id = resp_json.get("id")
        assert created_product_id is not None, "Created product ID not found in response"
        # Further optional assertions to verify data integrity can be added here
    finally:
        # Clean up: delete the created product if it was created
        if created_product_id:
            delete_url = f"{product_url}/{created_product_id}"
            try:
                delete_response = requests.delete(delete_url, headers=headers, timeout=30)
                # Deletion may respond with 204 or 200 or 404, so accept these
                assert delete_response.status_code in [200, 204, 404], \
                    f"Unexpected status code on delete: {delete_response.status_code}"
            except Exception as e:
                # Log or raise on deletion failure if needed
                pass

test_post_api_products_with_valid_data()