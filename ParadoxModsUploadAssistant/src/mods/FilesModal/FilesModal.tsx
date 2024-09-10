import React, { useState } from 'react';
import './FilesModal.scss';
import { bindValue, trigger, useValue } from 'cs2/api';
import mod from '../../../mod.json';
import { useCustomMenuState } from 'mods/Panel/CustomStateMenuHook';

// Bind the ThumbnailImage$ value
export const ThumbnailImage$ = bindValue<string>(mod.id, 'Thumbnail');
export const UploadStatus$ = bindValue<string>(mod.id, 'UploadStatus');


const FilesModal = () => {
    const { isOpened, setIsOpened } = useCustomMenuState();
    // Use useValue to get the current thumbnail image from a reactive source
    const ThumbnailImage = useValue(ThumbnailImage$) || 'https://i.imgur.com/42B3e4O.png'; // Fallback URL
    const UploadStatus = useValue(UploadStatus$);

    // Use useState to manage local changes to the thumbnail image
    const [newImage, setNewImage] = useState<string>(ThumbnailImage);

    // Function to handle thumbnail changes
    const handleThumbnailChange = (newImageUrl: string) => {
        setNewImage(newImageUrl); // Update local state
    };



    // Function to handle title input changes
    const HandleTitle = (event: React.ChangeEvent<HTMLInputElement>) => {
        const title = event.target.value; // Extract the value from the input
        trigger(mod.id, 'SendTitle', title); // Pass the title value to the trigger function
    };

    // Function to handle modal exit with error handling
    const handleExitClick = () => {
        try {
            setIsOpened(false); // Set isOpened to false
        } catch (error) {
            console.error("Error while closing the modal:", error); // Log the error
        }
    };


    const HandleSelectItemsClick = () => {
        trigger(mod.id, 'HandleSelectItemsClick');
    }
    // Function to handle thumbnail updates
    const HandleThumbnail = () => {
        // Trigger the thumbnail change and send the new thumbnail to the backend
        handleThumbnailChange(newImage);
        trigger(mod.id, 'SendThumbnail'); // Trigger the SendThumbnail action
    };

    const Upload = () => {
        trigger(mod.id, 'Upload'); // Trigger the SendThumbnail action
    }

    return (
        <div className='PDXMODSASSISTANTPANEL2'>
            {/* Thumbnail update button */}
            <button
    className='ThumbUpdate'
    onClick={HandleThumbnail} // Correctly placed on the button element
>
    <img
        src='https://cdn-icons-png.flaticon.com/512/81/81081.png'
        alt='Update Thumbnail' // Added alt attribute for accessibility
    />

</button>


            {/* Display the current thumbnail */}
            <button>
                <img
                    className='ThumbnailImage'
                    src={`${newImage}?t=${new Date().getTime()}`} // Append timestamp to force reload
                    alt='Thumbnail'
                    key={newImage} // Force re-render when newImage changes
                />
            </button>

            {/* Title label and input */}
            <h1 className='TitleLabel'>Title</h1>
            <input 
                onChange={HandleTitle}
                className='toggle_cca item-mouse-states_Fmi toggle_th_ TitleInput' 
                type='text'
                placeholder="Enter title"
            />

            {/* Other buttons */}
            <button className='button_uFa child-opacity-transition_nkS UploadButton'
                onClick={Upload}
            >
                Upload
            </button>
            <button 
    className='button_uFa child-opacity-transition_nkS SelectItemsButton'
    onClick={HandleSelectItemsClick}
>
    Select Items
            </button>
            
            <h1
            className='UploadText'
            > {UploadStatus}</h1>

        </div>
    );
};

export default FilesModal;
