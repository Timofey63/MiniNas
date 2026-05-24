async function loadFiles() {
    const res = await fetch('/files');
    const files = await res.json();

    const list = document.getElementById('fileList');
    list.innerHTML = '';

    files.forEach(f => {
        const li = document.createElement('li');

        const link = document.createElement('a');
        link.href = '/download/' + f;
        link.innerText = f;

        const delBtn = document.createElement('button');
        delBtn.innerText = 'x';
        delBtn.onclick = async () => {
            await fetch('/delete/' + f, { method: 'DELETE' });
            loadFiles();
        };

        li.appendChild(link);
        li.appendChild(delBtn);

        list.appendChild(li);
    });
}

document.getElementById('uploadForm').onsubmit = async (e) => {
    e.preventDefault();

    const formData = new FormData(e.target);

    await fetch('/upload', {
        method: 'POST',
        body: formData
    });

    loadFiles();
};

loadFiles();