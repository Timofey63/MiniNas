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


        const playBtn = document.createElement('button');
        playBtn.innerText = 'play';
        playBtn.onclick = () => playVideo(f);

        const delBtn = document.createElement('button');
        delBtn.innerText = 'x';
        delBtn.onclick = async () => {
            await fetch('/delete/' + f, { method: 'DELETE' });
            loadFiles();
        };

        li.appendChild(playBtn);
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

function playVideo(filename) {
    const player = document.getElementById('player');

    player.src = '/stream/' + filename;
    player.style.display = 'block';

    player.play();
}

loadFiles();