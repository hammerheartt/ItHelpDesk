import { Link } from 'react-router'

export default function NotFoundPage() {
  return (
    <>
      <h1>Page not found</h1>
      <div className="panel">
        <Link to="/">Back to start</Link>
      </div>
    </>
  )
}