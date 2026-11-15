import { useEffect, useState } from 'react'

export default function App() {
    const [status, setStatus] = useState('Checking API...')

    useEffect(() => {
        fetch('/api/categories')
            .then(response => setStatus(`API answered with status ${response.status}`))
            .catch(() => setStatus('API not reachable'))
    }, [])

    return (
        <div className="panel" style={{ margin: 40 }}>
            <h1>ItHelpDesk</h1>
            <p>{status}</p>
        </div>
    )
}