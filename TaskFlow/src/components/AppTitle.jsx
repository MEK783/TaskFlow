import taskflow from "/TaskFlow.png"

export default function AppTitle() {
    return (
        <div className="flex flex-row items-start ml-2 mr-4">
            <img src={taskflow} alt="TaskFlow App" className="p-1 w-13 h-13 min-w-10 min-h-10 object-contain mr-2" />
            <h1 className="font-exo text-5xl italic font-extrabold dark:text-white" >TaskFlow</h1>
        </div>
    )
}