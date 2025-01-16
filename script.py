# import threading
# import requests

# def make_request(path):
#     try:
#         response = requests.get(f"http://localhost:4221{path}")
#         print(f"Path: {path}, Response: {response.text}")
#     except Exception as e:
#         print(f"Error: {e}")

# threads = []
# paths = ["/echo/Thread1", "/echo/Thread2", "/echo/Thread3", "/echo/Thread4","/echo/Thread5","/echo/Thread6","/echo/Thread7","/echo/Thread8","/echo/Thread9","/echo/Thread10"]

# for path in paths:
#     thread = threading.Thread(target=make_request, args=(path,))
#     threads.append(thread)
#     thread.start()

# for thread in threads:
#     thread.join()
import threading
import requests

def make_request(path, headers, results, index):
    try:
        response = requests.get(f"http://localhost:4221{path}", headers=headers)
        results[index] = f"Path: {path}, Response: {response.status_code} - {response.text} - {response.headers}"
    except Exception as e:
        results[index] = f"Path: {path}, Error: {e}"

def send_requests(concurrent_requests, path_template, headers):
    threads = []
    results = [None] * concurrent_requests  # Preallocate space for results
    for i in range(concurrent_requests):
        path = path_template.format(i=i + 1)
        thread = threading.Thread(target=make_request, args=(path, headers, results, i))
        threads.append(thread)
        thread.start()

    for thread in threads:
        thread.join()

    # Print results in order
    for result in results:
        print(result)

if __name__ == "__main__":
    # Prompt the user for input
    concurrent_requests = int(input("Enter the number of concurrent requests to send: "))
    path_template = input("Enter the path template (use {i} for unique request numbering, e.g., '/echo/Message{i}'): ")
    
    # Define headers
    headers = {}
    while True:
        key = input("Enter header key (or press Enter to finish): ").strip()
        if not key:
            break
        value = input(f"Enter value for header '{key}': ").strip()
        headers[key] = value

    # Start sending requests
    send_requests(concurrent_requests, path_template, headers)

