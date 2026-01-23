
import React from 'react'
import ReactQuill from 'react-quill'
import 'react-quill/dist/quill.snow.css'

export default function RichTextEditor({ value, onChange }){
  return (
    <div className="prose max-w-none dark:prose-invert">
      <ReactQuill theme="snow" value={value} onChange={onChange} />
    </div>
  )
}
