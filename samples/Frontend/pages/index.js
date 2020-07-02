import fetch from 'isomorphic-unfetch'

import { body, header, button } from './index.styles';

const Index = ({ title }) => (
  <div>
    <h1>{title}</h1>
    <style jsx>{header}</style>
    <style jsx>{button}</style>
    <style jsx global>{body}</style>
  </div>
)

Index.getInitialProps = async function() {
  const res = await fetch('http://localhost:5000')
  const data = await res.json()

  console.log(`Show data fetched. data: ${JSON.stringify(data)}`)

  return {
    title: data.title
  }
}

export default Index